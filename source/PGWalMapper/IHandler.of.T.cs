namespace PGWalMapper {
    using System;
    using System.Threading.Tasks;

    public interface IAsyncHandler<in T> {
        Task HandleAsync(T obj);
    }

    public class AsyncHandler<T> : IAsyncHandler<T> {
        private readonly Func<T, Task<T>> _func;

        public AsyncHandler(Func<T, Task<T>> func) {
            _func = func;
        }

        public Task HandleAsync(T obj) => _func.Invoke(obj);
    }
}