namespace PGWalMapper {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class WalConfigurationBuilder {
        private readonly string _connectionString;
        private string _publicationName;
        private string _slotName;
        private readonly HashSet<IClassMapping> _classMaps = new();
        private readonly HashSet<Action<object>> _actions = new();

        public WalConfigurationBuilder(string connectionString) {
            _connectionString = connectionString;
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
            _classMaps.Add(cm);
            return cm;
        }

        /// <summary>
        /// Think of this like the $all stream within EventStore.  Any insert/update/delete operation will call this method.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public WalConfigurationBuilder On(Action<object> action) {
            _actions.Add(action);
            return this;
        }


        /// <summary>
        /// Think of this like the $all stream within EventStore.  Any insert/update/delete operation will call this method.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public WalConfigurationBuilder On<TEvent>(Action<TEvent> action) {
            _actions.Add(o => action((TEvent)o));
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
            if (_actions.Count == 0) exceptions.Add(new Exception("No actions have been defined."));
            if (_classMaps.Count == 0) exceptions.Add(new Exception("No classes have been mapped."));

            foreach (var cm in _classMaps) {
                cm.Validate(exceptions);
            }

            if (exceptions.Count == 1) throw exceptions.First();
            if (exceptions.Count > 1) throw new AggregateException(exceptions);

            return new WalListener(
                _connectionString,
                _publicationName,
                _slotName,
                _classMaps,
                _actions);
        }
    }
}