namespace PostgresWalMapper {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Npgsql.Replication;
    using Npgsql.Replication.PgOutput;
    using Npgsql.Replication.PgOutput.Messages;

    internal class InstanceConstructor {
        private readonly bool _createInstanceWithConstructorArgs;
        private readonly Dictionary<string, Type> _parameterNames = new(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger _logger;
        private readonly Type _objectType;
        private readonly string _schemaName;
        private readonly string _tableName;
        private readonly Dictionary<string, string> _propertyNameToColumn;

        public InstanceConstructor(Type objectType, IClassMapping cm, ILogger logger) {
            _objectType = objectType;
            _logger = logger;
            _schemaName = cm.SchemaName;
            _tableName = cm.TableName;

            _propertyNameToColumn = cm.Columns.Select(c => new { c.ColumnName, c.PropertyName })
                .ToDictionary(x => x.PropertyName, x => x.ColumnName, StringComparer.InvariantCultureIgnoreCase);

            if (cm.ColumnsOnConstructor.Any()) {
                _createInstanceWithConstructorArgs = true;

                // find constructor with all field names.  For now, we'll assume types are 1-1.
                var constructors = objectType.GetConstructors();
                foreach (var c in constructors) {
                    var parameters = c.GetParameters();

                    var query = from colMap in cm.ColumnsOnConstructor
                        join param in parameters on colMap.PropertyName.ToUpperInvariant() equals param.Name?.ToUpperInvariant() into foundParam
                        from subParam in foundParam.DefaultIfEmpty()
                        select new { ColumnMap = colMap, ParameterMap = subParam };

                    if (query.All(q => q.ParameterMap != null)) {
                        // constructor looks legit.  let's extract out the parameters, and we'll be able to use this for name(s) of the value(s) of the passed record from pgsql
                        parameters.Apply(x => _parameterNames.Add(x.Name, x.ParameterType));
                    }
                }

                if (_parameterNames.Count == 0) {
                    _logger.LogCritical("Parameter names could not be resolved for '{@NameOfObject}'", objectType.Name);
                    throw new Exception("Could not map constructor parameters to column mappings.");
                }
            }
        }

        private static readonly IEnumerable<Type> ImplementedMessageTypes = new[] { typeof(InsertMessage), typeof(UpdateMessage), typeof(FullDeleteMessage), typeof(KeyDeleteMessage) };
        public bool CanCreateInstance(ReplicationMessage msg) => ImplementedMessageTypes.Any(imt => imt.IsInstanceOfType(msg));

        public async Task<object> CreateInstance(ReplicationMessage msg, CancellationToken token = default) {
            switch (msg) {
                case InsertMessage inserted:
                    return await Parse(inserted.Relation, inserted.NewRow, token);
                case UpdateMessage updated:
                    return await Parse(updated.Relation, updated.NewRow, token);
                case FullDeleteMessage fullDelete:
                    return await Parse(fullDelete.Relation, fullDelete.OldRow, token);
                case KeyDeleteMessage keyDelete:
                    return await Parse(keyDelete.Relation, keyDelete.Key, token);
            }

            throw new ParsingReplicationMessagedFailedException();
        }


        private async Task<object> Parse(RelationMessage rel, ReplicationTuple tuple, CancellationToken token = default) {
            _logger.LogTrace("Parsing the relation details");
            if (!rel.Namespace.Equals(_schemaName) || !rel.RelationName.Equals(_tableName)) {
                _logger.LogWarning("Could not parse relation message for {@SchemaName} - {@TableName}", _schemaName, _tableName);
                return null;
            }

            object instance;
            var columns = rel.Columns.Select(c => c.ColumnName).ToArray();
            var columnValues = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            var dbColumnValues = tuple.GetAsyncEnumerator(token);
            foreach (var c in columns) {
                await dbColumnValues.MoveNextAsync();
                columnValues.Add(c, await dbColumnValues.Current.Get(token));
            }


            if (_createInstanceWithConstructorArgs) {
                var parameterValues = new List<object>();
                foreach (var cpa in _parameterNames) {
                    var columnName = _propertyNameToColumn[cpa.Key];
                    if (columnValues.ContainsKey(columnName)) {
                        var valueOfColumn = columnValues[columnName];
                        parameterValues.Add(valueOfColumn is DBNull
                            ? null
                            : Convert.ChangeType(valueOfColumn, cpa.Value));
                    }
                }

                instance = Activator.CreateInstance(_objectType, parameterValues.ToArray());
            } else {
                instance = Activator.CreateInstance(_objectType);
            }
            // instance is now built

            // need to now populate its properties that are writable.
            var properties = instance.GetType().GetProperties().Where(p => p.CanWrite).ToArray();
            foreach (var p in properties) {
                var val = columnValues[_propertyNameToColumn[p.Name]];
                if (val is DBNull) continue;
                p.SetValue(instance, Convert.ChangeType(val, p.PropertyType));
            }

            // need to now populate its fields that are writable.
            var fields = instance.GetType().GetFields().Where(f => !f.IsInitOnly);
            foreach (var p in fields) {
                var val = columnValues[_propertyNameToColumn[p.Name]];
                if (val is DBNull) continue;
                p.SetValue(instance, Convert.ChangeType(val, p.FieldType));
            }

            return instance;
        }
    }
}