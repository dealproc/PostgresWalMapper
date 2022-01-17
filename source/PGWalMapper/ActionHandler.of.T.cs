namespace PGWalMapper {
    using System;

    public class ActionHandler {
        private readonly Action<object> _action;
        protected Type MatchingType;

        public ActionHandler(Action<object> actionToPerform) {
            _action = actionToPerform;
            MatchingType = actionToPerform.GetType().GetGenericArguments()[0];
        }

        public virtual bool CanHandle(object obj) => MatchingType.IsInstanceOfType(obj);
        public virtual void Handle(object obj) => _action.Invoke(obj);
    }

    public class ActionHandler<T> : ActionHandler {
        public ActionHandler(Action<T> actionToPerform) : base((o) => actionToPerform((T)o)) {
            MatchingType = typeof(T);
        }
    }
}