namespace PGWalMapper {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IClassMapping {
        public string TableName { get; }
        public string SchemaName { get; }
        ICollection<IColumnMapping> Columns { get; }
        public IEnumerable<IColumnMapping> ColumnsOnConstructor { get; }
        public void Validate(List<Exception> possibleExceptions);
        // public bool Handle(ReplicationMessage msg);
    }

    public class ClassMapping<TEvent> : IClassMapping where TEvent : class {
        private readonly WalConfigurationBuilder _builder;
        private readonly OrderedSet<ColumnMapping<TEvent>> _columnMappings = new();
        public string TableName { get; private set; }

        private string _schemaName;

        public string SchemaName {
            get => string.IsNullOrWhiteSpace(_schemaName) ? "public" : _schemaName;
            private set => _schemaName = value;
        }


        public ICollection<IColumnMapping> Columns => _columnMappings.Cast<IColumnMapping>().ToList();
        public IEnumerable<IColumnMapping> ColumnsOnConstructor => _columnMappings.Where(cm => cm.SetOnConstructor);

        internal ClassMapping(WalConfigurationBuilder builder) {
            _builder = builder;
        }

        /// <summary>
        /// Defines which table should be observed for this class mapping.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public ClassMapping<TEvent> ToTable(string tableName) {
            TableName = tableName;
            return this;
        }

        /// <summary>
        /// Defines which schema the table should be within.  If not set, then the assumption of `public` will be
        /// used.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public ClassMapping<TEvent> InSchema(string schemaName) {
            SchemaName = schemaName;
            return this;
        }

        /// <summary>
        /// Starts the definition of a column to property/member variable.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public ColumnMapping<TEvent> Column(string columnName) {
            var cm = new ColumnMapping<TEvent>(columnName, this, _builder);
            _columnMappings.Add(cm);
            return cm;
        }

        /// <summary>
        /// When a new record is found on the WAL, and the data has been mapped to a CLR object, this
        /// then allows an ability to take an action on the built CLR object.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ClassMapping<TEvent> On(Action<object> action) {
            _builder.On(action);
            return this;
        }

        /// <summary>
        /// When a new record is found on the WAL, and the data has been mapped to a CLR object, this
        /// then allows an ability to take an action on the built CLR object.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ClassMapping<TEvent> On(Action<TEvent> action) {
            _builder.On(action);
            return this;
        }

        /// <summary>
        /// Starts to define a mapping of a database record to the specified CLR object.
        /// </summary>
        /// <typeparam name="TNextEvent"></typeparam>
        /// <returns></returns>
        public ClassMapping<TNextEvent> Map<TNextEvent>() where TNextEvent : class => _builder.Map<TNextEvent>();

        /// <summary>
        /// builds the final listener after the configuration has completed.
        /// </summary>
        /// <returns></returns>
        public WalListener Build() => _builder.Build();

        public void Validate(List<Exception> possibleExceptions) {
            if (_columnMappings.Count == 0) {
                possibleExceptions.Add(new Exception($"No columns have been mapped for {typeof(TEvent).Name}"));
                return;
            }

            foreach (var cm in _columnMappings) {
                cm.Validate(possibleExceptions);
            }
        }
    }
}