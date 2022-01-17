namespace PGWalMapper {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal static class EnumerableExtensions {
        public static IEnumerable<T> Apply<T>(this IEnumerable<T> items, Action<T> method) {
            foreach (var item in items) {
                method.Invoke(item);
            }

            return items;
        }

        public static async Task<IEnumerable<T>> ApplyAsync<T>(this IEnumerable<T> items, Func<T, Task> func) {
            await Task.WhenAll(items.Select(func));
            return items;
        }
    }
}