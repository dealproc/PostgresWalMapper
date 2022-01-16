namespace PGWalMapper {
    using System;

    public class ActionHandler {
        private Action<object> _action;
        private Type _matchingType;

        public ActionHandler(Action<object> actionToPerform) {
            _action = actionToPerform;
            _matchingType = actionToPerform.GetType().GetGenericArguments()[0];
        }

        public virtual bool CanHandle(object obj) => obj.GetType().IsAssignableTo(_matchingType);
        public virtual void Handle(object obj) => _action.Invoke(obj);
    }

    public class ActionHandler<T> : ActionHandler {
        public ActionHandler(Action<T> actionToPerform) : base((o) => actionToPerform((T)o)) { }
    }
}