using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serie1Pc
{
    public class ThrottledRegion
    {
        private object lockObj = new object();
        private Dictionary<int, MyObject> map;
        private int maxInside;
        private int maxWaiting;
        private int waitTimeout;

        public ThrottledRegion(int maxInside, int maxWaiting, int waitTimeout)
        {
            map = new Dictionary<int, MyObject>();
            this.maxInside = maxInside;
            this.maxWaiting = maxWaiting;
            this.waitTimeout = waitTimeout;
        }

        public bool TryEnter(int key)
        {
            return false;
        }
    }
}
        
