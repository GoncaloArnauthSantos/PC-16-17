using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serie1Pc
{
    class MyObject
    {
        public int permits;
        private int maxWaiting;
        private readonly LinkedList<int> lst;

        public MyObject(int permits, int maxWaiting)
        {
            lst = new LinkedList<int>();
            this.permits = permits;
            this.maxWaiting = maxWaiting;
        }
    }
}
