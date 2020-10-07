﻿using System;
using System.Collections.Generic;

namespace SuperComicLib.Collections
{
    public static partial class IEnumerableExtension
    {
        public static IEnumerable<T> FastSelect<T>(this IEnumerable<T> collection, T item, IEqualityComparer<T> comparer) =>
            new Selector<T>(item, collection.GetEnumerator(), comparer);

        public static IEnumerable<T> FastSelect<T>(this IEnumerable<T> collection, T item) =>
            FastSelect(collection, item, EqualityComparer<T>.Default);

        public static IEnumerable<T> FastClone<T>(this IEnumerable<T> collection, Func<T, T> callback) =>
            new ElementCloner<T>(collection.GetEnumerator(), callback);


        public static bool ContainsAll<T>(this IEnumerable<T> set, IEnumerable<T> items)
        {
            IEnumerator<T> src = set.GetEnumerator();
            IEnumerator<T> other = items.GetEnumerator();

            IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

            while (other.MoveNext() && src.MoveNext())
                if (!comparer.Equals(other.Current, src.Current))
                {
                    src.Dispose();
                    other.Dispose();

                    return false;
                }

            src.Dispose();
            other.Dispose();

            return true;
        }
    }
}
