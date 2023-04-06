using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class LinqHelper
    {
        public static bool IsNotNull<T>(T element)
        {
            return element != null;
        }

        public static bool IsObjectNull<T>(T element) where T : Object
        {
            return element == null;
        }
        
        public static bool IsObjectNotNull<T>(T element) where T : Object
        {
            return element != null;
        }

        public static bool IsNull<T>(T element)
        {
            return element == null;
        }

        public static bool IsEven<T>(T _, int idx)
        {
            return idx % 2 == 0;
        }

        public static bool IsOdd<T>(T _, int idx)
        {
            return idx % 2 != 0;
        }

        public static bool IsNotNegative(int value)
        {
            return value >= 0;
        }

        public static List<T> ToList<T>(this IEnumerator<T> source)
        {
            var list = new List<T>();
            while (source.MoveNext())
            {
                list.Add(source.Current);
            }
            return list;
        }
    }
}