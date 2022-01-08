namespace PGWalMapper {
    using System;
    using System.Formats.Asn1;

    public class WalConfigurationBuilder {
        public WalConfigurationBuilder WithConnectionString(string connectionString) {
            return this;
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