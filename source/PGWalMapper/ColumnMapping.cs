namespace PGWalMapper {
    using System;
    using System.Linq.Expressions;

    public class ColumnMapping<TEvent> {
        private readonly string _columnName;
        private readonly ClassMapping<TEvent> _classMapping;
        private readonly WalConfigurationBuilder _builder;

        public ColumnMapping(string columnName, ClassMapping<TEvent> classMapping, WalConfigurationBuilder builder) {
            _columnName = columnName;
            _classMapping = classMapping;
            _builder = builder;
        }

        public ColumnMapping<TEvent> ToProperty<TProp>(Expression<Func<TEvent, TProp>> expr) {
            return this;
        }
        
        public ColumnMapping<TEvent> AsConstructorArgument() {
            return this;
        }

        public ColumnMapping<TEvent> Column(string columnName) => new(columnName, _classMapping, _builder);

        public ClassMapping<TEvent> Map<TEvent>() => new(_builder);

        public ClassMapping<TEvent> On(Action<TEvent> action) {
            _builder.On(action);
            return _classMapping;
        }
    }
}