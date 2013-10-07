using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Updater
{
    public class Pair<T, E, G>
    {
        public T key { get; set; }
        public E value { get; set; }
        public G extra { get; set; }
        public Pair(T t, E e, G g)
        {
            key = t;
            value = e;
            extra = g;
        }
    }
}
