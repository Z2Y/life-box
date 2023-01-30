using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils {

    namespace Shuffle {

        static class ShuffleExtension {
            public static IList<T> Shuffle<T>(this IList<T> list) {
                Random rad = new Random();
                return list.Shuffle(rad);
            }

            public static IList<T> Shuffle<T>(this IList<T> list, Random random) {
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
                return list;             
            }

            public static void Swap<T>(this IList<T> list, int i, int j) {
                T value = list[i];
                list[i] = list[j];
                list[j] = value;
            }

        }
    }
}