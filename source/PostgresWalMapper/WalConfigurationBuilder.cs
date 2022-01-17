namespace PostgresWalMapper {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Extensions.Logging;

    using Npgsql.Replication;

    public class WalConfigurationBuilder {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private string _publicationName;
        private string _slotName;
        private readonly Dictionary<Type, IClassMapping> _classMaps = new();
        private readonly HashSet<ActionHandler> _insertActions = new();
        private readonly HashSet<ActionHandler> _updateActions = new();
        private readonly HashSet<ActionHandler> _deleteActions = new();

        public WalConfigurationBuilder(string connectionString, ILogger logger) {
            _connectionString = connectionString;
            _logger = logger;
        }

        /// <summary>
        /// Sets the publication name within pgsql to be observing.
        /// </summary>
        /// <param name="publicationName"></param>
        /// <returns></returns>
        public WalConfigurationBuilder ForPublication(string publicationName) {
            _publicationName = publicationName;
            return this;
        }

        /// <summary>
        /// Sets the "slot" within pgsql that will be used to keep track of the position of the database operation change.
        /// </summary>
        /// <param name="slotName"></param>
        /// <returns></returns>
        public WalConfigurationBuilder UsingSlot(string slotName) {
            _slotName = slotName;
            return this;
        }

        /// <summary>
        /// Starts the building of an event mapper.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public ClassMapping<TEvent> Map<TEvent>() where TEvent : class {
            var cm = new ClassMapping<TEvent>(this);
            _classMaps.Add(typeof(TEvent), cm);
            return cm;
        }

        public WalConfigurationBuilder OnInsert(Action<object> action) {
            _insertActions.Add(new ActionHandler(action));
            return this;
        }

        public WalConfigurationBuilder OnInsert<TEvent>(Action<TEvent> action) {
            _insertActions.Add(new ActionHandler<TEvent>(action));
            return this;
        }

        public WalConfigurationBuilder OnUpdate(Action<object> action) {
            _updateActions.Add(new ActionHandler(action));
            return this;
        }

        public WalConfigurationBuilder OnUpdate<TEvent>(Action<TEvent> action) {
            _updateActions.Add(new ActionHandler<TEvent>(action));
            return this;
        }

        public WalConfigurationBuilder OnDelete(Action<object> action) {
            _deleteActions.Add(new ActionHandler(action));
            return this;
        }

        public WalConfigurationBuilder OnDelete<TEvent>(Action<TEvent> action) {
            _deleteActions.Add(new ActionHandler<TEvent>(action));
            return this;
        }

        /// <summary>
        /// Builds the final listener.
        /// </summary>
        /// <returns></returns>
        public WalListener Build() {
            var exceptions = new List<Exception>();

            if (string.IsNullOrWhiteSpace(_connectionString)) exceptions.Add(new Exception("Connection string is empty."));
            if (string.IsNullOrWhiteSpace(_publicationName)) exceptions.Add(new Exception("Publication name has not been set."));
            if (string.IsNullOrWhiteSpace(_slotName)) exceptions.Add(new Exception("Slot name has not been set."));
            if (_insertActions.Count == 0) exceptions.Add(new Exception("No actions have been defined."));
            if (_classMaps.Count == 0) exceptions.Add(new Exception("No classes have been mapped."));

            foreach (var cm in _classMaps) {
                cm.Value.Validate(exceptions);
            }

            if (exceptions.Count == 1) throw exceptions.First();
            if (exceptions.Count > 1) throw new AggregateException(exceptions);

            var builders = _classMaps.Select(cm => new InstanceConstructor(cm.Key, cm.Value, _logger));

            return new WalListener(
                new LogicalReplicationConnection(_connectionString),
                _publicationName,
                _slotName,
                builders,
                _insertActions,
                _updateActions,
                _deleteActions,
                _logger
            );
        }
    }
}