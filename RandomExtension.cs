using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstPlugin
{
    static class RandomExtension
    {
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}
