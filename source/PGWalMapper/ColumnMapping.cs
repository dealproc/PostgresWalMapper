namespace PGWalMapper {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public interface IColumnMapping {
        string ColumnName { get; }
        string PropertyName { get; }
        bool SetOnConstructor { get; }
    }
    
    /// <summary>
    /// Maps a pgsql column to a property on a <see cref="{TEvent}"/>
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public class ColumnMapping<TEvent> : IColumnMapping where TEvent : class {
        private readonly ClassMapping<TEvent> _classMapping;
        private readonly WalConfigurationBuilder _builder;

        public string ColumnName { get; }
        public string PropertyName { get; private set; }
        public bool SetOnConstructor { get; private set; } = false;
        
        /// <summary>
        ///  Instantiates a new instance of <see cref="ColumnMapping{TEvent}"/>
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="classMapping"></param>
        /// <param name="builder"></param>
        internal ColumnMapping(string columnName, ClassMapping<TEvent> classMapping, WalConfigurationBuilder builder) {
            ColumnName = columnName;
            _classMapping = classMapping;
            _builder = builder;
        }

        /// <summary>
        /// When mapping the postgres value into a CLR object, this is the property/member variable to be set from the pgsql field.
        /// </summary>
        /// <param name="expr"></param>
        /// <typeparam name="TProp"></typeparam>
        /// <returns></returns>
        public ColumnMapping<TEvent> ToProperty<TProp>(Expression<Func<TEvent, TProp>> expr) {
            var member = expr.Body as MemberExpression;
            if (member == null) throw new ArgumentException($"Expression '{expr.ToString()}' refers to a method, not a property.");

            if (member.Member is PropertyInfo pInfo) {
                PropertyName = pInfo.Name;
            } else if (member.Member is FieldInfo fInfo) {
                PropertyName = fInfo.Name;
            } else {
                throw new Exception("Invalid expression. Ensure that either a member variable or property is being mapped.");
            }
            
            return this;
        }
        
        /// <summary>
        /// Calling this method states that creating a new instance of <see cref="TEvent"/> will be using
        /// this value on the constructor of the instance being built.  For now, the assumption is that the
        /// name of the parameter on the constructor is the same as the member variable or property.
        /// </summary>
        /// <returns></returns>
        public ColumnMapping<TEvent> AsConstructorArgument() {
            SetOnConstructor = true;
            return this;
        }

        /// <summary>
        /// The column on the pgsql table to be mapped.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public ColumnMapping<TEvent> Column(string columnName) => _classMapping.Column(columnName);

        /// <summary>
        /// Starts a new class mapping definition.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public ClassMapping<TEvent> Map<TEvent>() where TEvent : class => _builder.Map<TEvent>();

        /// <summary>
        /// When an event comes in from WAL for the mapped object, this allows a reactive methodology to be
        /// performed (e.g. put the object onto a stream.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ClassMapping<TEvent> On(Action<TEvent> action) {
            _builder.On(action);
            return _classMapping;
        }

        internal void Validate(List<Exception> possibleExceptions) {
            if(string.IsNullOrEmpty(ColumnName)) possibleExceptions.Add(new Exception($"Column name is blank or has not been set for {typeof(TEvent).Name}"));
            if(string.IsNullOrWhiteSpace(PropertyName)) possibleExceptions.Add(new Exception($"Property has not been set for '{typeof(TEvent)}', '{ColumnName}'"));
        }

    }
}