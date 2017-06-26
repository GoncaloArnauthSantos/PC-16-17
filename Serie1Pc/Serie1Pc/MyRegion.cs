using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serie1Pc
{
    class MyRegion
    {
        public int permits; // quantos permite dentro da regiao 
        private int maxWaiting; // nº de threads à espera fora da regiao
        private readonly LinkedList<int> waittingList; // lista de espera FIFO

        public MyRegion(int permits, int maxWaiting)
        {
            waittingList = new LinkedList<int>();
            this.permits = permits;
            this.maxWaiting = maxWaiting;
        }
        public bool IsFirst(int id)
        {
            return waittingList.First.Value == id;
        }

        public void RemoveWaiter(int id)
        {
            maxWaiting++;
            waittingList.Remove(id);
        }
        public bool AddWaiter(int id)
        {
            if( maxWaiting > 0)
            {
                maxWaiting--;
                waittingList.AddLast(id);
                return true;
            }
            return false;
        }
    }
}
