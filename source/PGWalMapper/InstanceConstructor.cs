namespace PGWalMapper {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Npgsql.Replication;
    using Npgsql.Replication.PgOutput;
    using Npgsql.Replication.PgOutput.Messages;

    public class InstanceConstructor {
        private readonly bool _createInstanceWithConstructorArgs;
        private readonly List<string> _parameterNames = new();
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
                .ToDictionary(x => x.PropertyName, x => x.ColumnName);
            
            Console.WriteLine($"Property Names: {_propertyNameToColumn.Keys.Aggregate((s1, s2) => $"{s1},{s2}")}");
            Console.WriteLine($"Column Names: {_propertyNameToColumn.Values.Aggregate((s1, s2) => $"{s1},{s2}")}");

            if (cm.ColumnsOnConstructor.Any()) {
                _createInstanceWithConstructorArgs = true;

                // find constructor with all field names.  For now, we'll assume types are 1-1.
                var constructors = objectType.GetConstructors();
                foreach (var c in constructors) {
                    var parameters = c.GetParameters();

                    var query = from colMap in cm.ColumnsOnConstructor
                        join param in parameters on colMap.PropertyName equals param.Name into foundParam
                        from subParam in foundParam.DefaultIfEmpty()
                        select new { ColumnMap = colMap, ParameterMap = subParam };

                    if (query.All(q => q.ParameterMap != null)) {
                        // constructor looks legit.  let's extract out the parameters, and we'll be able to use this for name(s) of the value(s) of the passed record from pgsql
                        _parameterNames.AddRange(parameters.Select(p => p.Name));
                    }
                }

                if (_parameterNames.Count == 0) throw new Exception("Could not map constructor parameters to column mappings.");
            }
        }

        public async Task<object> CreateInstance(ReplicationMessage msg, CancellationToken token = default) {
            Console.WriteLine($"Object instance is being created...{msg.GetType().Name}");
            switch (msg) {
                case InsertMessage inserted:
                    Console.WriteLine("Parsing inserted message");
                    return await Parse(inserted.Relation, inserted.NewRow, token);
                case UpdateMessage updated:
                    Console.WriteLine("Parsing updated message");
                    return await Parse(updated.Relation, updated.NewRow, token);
                case FullDeleteMessage fullDelete:
                    Console.WriteLine("Parsing deleted message");
                    return await Parse(fullDelete.Relation, fullDelete.OldRow, token);
                case KeyDeleteMessage keyDelete:
                    Console.WriteLine("Parsing deleted message");
                    return await Parse(keyDelete.Relation, keyDelete.Key, token);
            }

            throw new ParsingReplicationMessagedFailedException();
        }


        private async Task<object> Parse(RelationMessage rel, ReplicationTuple tuple, CancellationToken token = default) {
            Console.WriteLine("Entered parsing alg.");
            if (!rel.Namespace.Equals(_schemaName) || !rel.RelationName.Equals(_tableName)) {
                //_logger.LogDebug("Could not parse relation message for {@SchemaName} - {@TableName}", _schemaName, _tableName);
                return null;
            }
            
            Console.WriteLine("Found namespace/name");
            
            object instance;
            var columns = rel.Columns.Select(c => c.ColumnName).ToArray();
            var columnValues = new Dictionary<string, object>();
            var dbColumnValues = tuple.GetAsyncEnumerator(token);
            foreach (var c in columns) {
                await dbColumnValues.MoveNextAsync();
                columnValues.Add(c, await dbColumnValues.Current.Get(token));
                Console.WriteLine($"Column name: {c}");
            }
            
            Console.WriteLine("Pulled values");

            if (_createInstanceWithConstructorArgs) {
                var parameterValues = new List<object>();
                foreach (var cpa in _parameterNames) {
                    if (columnValues.ContainsKey(cpa)) {
                        parameterValues.Add(columnValues[cpa]);
                    }
                }

                instance = Activator.CreateInstance(_objectType, parameterValues.ToArray());
            } else {
                instance = Activator.CreateInstance(_objectType);
            }
            Console.WriteLine("Built instance.");

            // instance is now built

            // need to now populate its properties that are writable.
            var properties = instance.GetType().GetProperties().Where(p => p.CanWrite).ToArray();
            Console.WriteLine($"Database Names: {columnValues.Keys.Aggregate((s1, s2)=> $"{s1}, {s2}")}");
            Console.WriteLine($"Database Values: {columnValues.Values.Aggregate((s1, s2)=> $"{s1}, {s2}")}");
            foreach (var p in properties) {
                var val = columnValues[_propertyNameToColumn[p.Name]];
                if (val is DBNull) continue;
                Console.WriteLine(val.ToString());
                p.SetValue(instance, Convert.ChangeType(val, p.PropertyType));
            }

            // need to now populate its fields that are writable.
            var fields = instance.GetType().GetFields().Where(f => !f.IsInitOnly);
            foreach (var p in fields) {
                var val = columnValues[_propertyNameToColumn[p.Name]];
                if (val is DBNull) continue;
                Console.WriteLine(val.ToString());
                p.SetValue(instance, Convert.ChangeType(val, p.FieldType));
            }
            
            Console.WriteLine("Assigned values");

            return instance;
        }
    }
}