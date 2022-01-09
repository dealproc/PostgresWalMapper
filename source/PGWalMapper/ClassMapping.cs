namespace PGWalMapper {
    using System;
    using System.Collections.Generic;

    using Npgsql.Replication;
    using Npgsql.Replication.PgOutput.Messages;

    internal interface IClassMapping {
        public void Validate(List<Exception> possibleExceptions);
        public bool Handle(ReplicationMessage msg);
    }

    public class ClassMapping<TEvent> : IClassMapping where TEvent : class {
        private readonly WalConfigurationBuilder _builder;
        private readonly HashSet<ColumnMapping<TEvent>> _columnMappings = new();
        private string _tableName;
        private string _schemaName;
        private SqlCommandTypes _commandType = SqlCommandTypes.Unknown;
        private IOperationHandler _operationHandler;

        internal ClassMapping(WalConfigurationBuilder builder) {
            _builder = builder;
        }

        /// <summary>
        /// Defines which table should be observed for this class mapping.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public ClassMapping<TEvent> ToTable(string tableName) {
            _tableName = tableName;
            return this;
        }

        /// <summary>
        /// Defines which schema the table should be within.  If not set, then the assumption of `public` will be
        /// used.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public ClassMapping<TEvent> InSchema(string schemaName) {
            _schemaName = schemaName;
            return this;
        }

        /// <summary>
        /// Defines that this class mapping is for insert operations.
        /// </summary>
        /// <returns></returns>
        public ClassMapping<TEvent> OnInsert() {
            _operationHandler = new InsertOperationHandler();
            return this;
        }

        /// <summary>
        /// Defines that this class mapping is for update operations.
        /// </summary>
        /// <returns></returns>
        public ClassMapping<TEvent> OnUpdate() {
            _operationHandler = new UpdateOperationHandler();
            return this;
        }

        /// <summary>
        /// Defines that this class mapping is for delete operations.
        /// </summary>
        /// <returns></returns>
        public ClassMapping<TEvent> OnDelete() {
            _operationHandler = new DeleteOperationHandler();
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
            if (_commandType == SqlCommandTypes.Unknown) possibleExceptions.Add(new Exception($"SQL Operation has not been set for '{typeof(TEvent).Name}'."));
            if (_columnMappings.Count == 0) {
                possibleExceptions.Add(new Exception($"No columns have been mapped for {typeof(TEvent).Name}"));
                return;
            }

            foreach (var cm in _columnMappings) {
                cm.Validate(possibleExceptions);
            }
        }

        public bool Handle(ReplicationMessage msg) {
            return _operationHandler.Handle(msg);
        }
    }
}