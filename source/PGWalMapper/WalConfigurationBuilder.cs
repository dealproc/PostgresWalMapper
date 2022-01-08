namespace PGWalMapper {
    using System;

    using Npgsql;

    public class WalConfigurationBuilder {
        private readonly NpgsqlConnection _connection;

        public WalConfigurationBuilder(NpgsqlConnection connection) {
            _connection = connection;
        }

        public WalConfigurationBuilder(string connectionString) {
            _connection = new NpgsqlConnection(connectionString);
        }

        public ClassMapping<TEvent> Map<TEvent>() {
            return new ClassMapping<TEvent>(this);
        }

        public WalConfigurationBuilder On(Action<object> action) {
            return this;
        }

        public WalConfigurationBuilder On<TEvent>(Action<TEvent> action) {
            return this;
        }

        public WalListener Build() {
            return new WalListener();
        }
    }
}