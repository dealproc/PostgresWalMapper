namespace PGWalMapper {
    using System;

    public class ClassMapping<TEvent> {
        private readonly WalConfigurationBuilder _builder;

        internal ClassMapping(WalConfigurationBuilder builder) {
            _builder = builder;
        }

        public ClassMapping<TEvent> ToTable(string tableName) {
            return this;
        }

        public ClassMapping<TEvent> InSchema(string schemaName) {
            return this;
        }

        public ClassMapping<TEvent> OnInsert() {
            return this;
        }

        public ClassMapping<TEvent> OnUpdate() {
            return this;
        }

        public ClassMapping<TEvent> OnDelete() {
            return this;
        }

        public ColumnMapping<TEvent> Column(string columnName) {
            return new ColumnMapping<TEvent>(columnName, this, _builder);
        }

        public ClassMapping<TEvent> On(Action<object> action) {
            _builder.On(action);
            return this;
        }

        public ClassMapping<TEvent> On(Action<TEvent> action) {
            _builder.On(action);
            return this;
        }

        public ClassMapping<TNextEvent> Map<TNextEvent>() {
            return new ClassMapping<TNextEvent>(_builder);
        }

        public WalListener Build() => _builder.Build();
    }
}