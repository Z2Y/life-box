using System;
using System.Buffers;
using System.Collections.Generic;
using StructLinq;
using StructLinq.IList;

namespace Utils
{
    public static class ListExtensions
    {
        public static ListEnumerable<T, IList<T>> ReadOnlyEnumerable<T>(this List<T> source)
        {
            return source.AsReadOnly().ToStructEnumerable();
        }
        
        public static List<T> AsList<T, TEnumerable, TEnumerator>(
            this TEnumerable enumerable,
            Func<TEnumerable, IStructEnumerable<T, TEnumerator>> _,
            int capacity = 0)
            where TEnumerable : IStructEnumerable<T, TEnumerator>
            where TEnumerator : struct, IStructEnumerator<T>
        {
            return new List<T>(enumerable.ToEnumerable());
        }
    }
}