using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Extensions
{
    /// <summary>
    /// Copied from https://stackoverflow.com/a/1262619
    /// </summary>
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random CurrentThreadsRandom => Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Environment.CurrentManagedThreadId)));
    }

    public static class ListExtensions
    {
        /// <summary>
        /// Shuffles the list elements randomly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int Count = list.Count;
            while (Count > 1)
            {
                --Count;
                int RandomIndex = ThreadSafeRandom.CurrentThreadsRandom.Next(Count + 1);
                T Temp = list[RandomIndex];
                list[RandomIndex] = list[Count];
                list[Count] = Temp;
            }
        }

        /// <summary>
        /// Shuffles the list elements randomly while preserving the order of elements out of the subset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="startingIndex">The beginning index of the subset.</param>
        /// <param name="endingIndex">The ending index of the subset.</param>
        public static void ShuffleSubset<T>(this IList<T> list, int startingIndex, int endingIndex)
        {
            int Count = endingIndex;
            while (Count > 1)
            {
                int RandomIndex = ThreadSafeRandom.CurrentThreadsRandom.Next(startingIndex, endingIndex + 1);
                int RandomIndex2 = ThreadSafeRandom.CurrentThreadsRandom.Next(startingIndex, endingIndex + 1);
                T Temp = list[RandomIndex];
                list[RandomIndex] = list[RandomIndex2];
                list[RandomIndex2] = Temp;
                --Count;
            }
        }
    }
}
