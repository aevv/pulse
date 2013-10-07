using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Pulse
{
    public class Pair<T, E>
    {
        public T key
        {
            get;
            set;
        }
        public E value
        {
            get;
            set;
        }
        public Pair(T t, E e)
        {
            key = t;
            value = e;
        }
    }
    public class PairComparer : IComparer<Pair<int,int>>
    {

        public int Compare(Pair<int, int> x, Pair<int, int> y)
        {
            if (x.key > y.key)
            {
                return 1;
            }
            else if (x.key < y.key)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
