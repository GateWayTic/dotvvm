#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
    public static class FunctionalExtensions
    {
        public static TValue GetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
            where TKey: notnull
            => dictionary[key];

#if CSharp8Polyfill // this method is actually present in .NET Standard
        [return: MaybeNull]
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, bool justAddAParameterSoCsharpDoesNotPreferThisMethodOverStandard = false)
            where TKey: notnull
        {
            dictionary.TryGetValue(key, out var value);
            return value;
        }
#endif

        public static TTarget ApplyAction<TTarget>(this TTarget target, Action<TTarget> outerAction)
        {
            outerAction(target);
            return target;
        }

        public static TResult Apply<TTarget, TResult>(this TTarget target, Func<TTarget, TResult> outerFunction)
            => outerFunction(target);

        public static T Assert<T>(this T target, Func<T, bool> predicate, string message = "A check has failed")
            => predicate(target) ? target : throw new Exception($"{message} | '{target?.ToString() ?? "null"}' checked by {GetDebugFunctionInfo(predicate)}]");

        private static string GetDebugFunctionInfo(Delegate func)
        {
            var funcName = $"{func.Method.DeclaringType!.FullName}.{func.Method.Name}";
            if (func.Target is null)
                return $"'{funcName}' without closure";
            var fields = func.Target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var fieldsFormatted = string.Join("; ", fields.Select(f => f.Name + ": " + f.GetValue(func.Target)?.ToString() ?? "null"));
            return $"'{funcName}' with closure [{fieldsFormatted}]";
        }

        public static TOut CastTo<TOut>(this object original)
            where TOut : class
            => (TOut)original;

        public static TOut? As<TOut>(this object? original)
            where TOut : class
            => original as TOut;

        public static IEnumerable<T> SelectRecursively<T>(this IEnumerable<T> enumerable, Func<T, IEnumerable<T>> children)
        {
            foreach (var e in enumerable)
            {
                yield return e;
                foreach (var ce in children(e).SelectRecursively(children))
                    yield return ce;
            }
        }

        public static string StringJoin(this IEnumerable<string> enumerable, string separator) =>
            string.Join(separator, enumerable);

        public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V value)
        {
            key = pair.Key;
            value = pair.Value;
        }
        public static IEnumerable<(int, T)> Indexed<T>(this IEnumerable<T> enumerable) =>
            enumerable.Select((a, b) => (b, a));

        public static T NotNull<T>(this T? target, string message = "Unexpected null value.")
            where T : class =>
            target ?? throw new Exception(message);
    }
}
