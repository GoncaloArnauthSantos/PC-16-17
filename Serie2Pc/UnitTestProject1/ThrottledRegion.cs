using System;
using System.Collections.Generic;
using System.Threading;

namespace Serie2Pc
{
    public class ThrottledRegion
    {
        private object lockObj = new object();
        private Dictionary<int, MyRegion> map;
        private int maxInside;
        private int maxWaiting;
        private int waitTimeout;

        public ThrottledRegion(int maxInside, int maxWaiting, int waitTimeout)
        {
            map = new Dictionary<int, MyRegion>();
            this.maxInside = maxInside;
            this.maxWaiting = maxWaiting;
            this.waitTimeout = waitTimeout;
        }

        public bool TryEnter(int key)
        {
            MyRegion mr;
            // fast path
            if(  map.TryGetValue(key,out mr))
            {
                if( mr.maxWaiting == maxWaiting)
                {
                    int per;
                    do
                    {
                        per = mr.permits;
                        if (per == 0)
                            break;
                        if( Interlocked.CompareExchange(ref mr.permits, per - 1, per) == per)
                            return true;

                    } while (true);
                }
            }

            // slow path
            lock (lockObj)
            {
                // se não fizer parte do map adiciona
                if (!map.ContainsKey(key)) 
                {
                    map.Add(key, new MyRegion(maxInside, maxWaiting));
                }
                MyRegion region = map[key]; // tem a regiao 

                // se a regiao ainda tiver permissao para mais incr e returna true(entra logo)
                if (region.permits > 0) 
                {
                    region.permits--;
                    return true; 
                }

                // se não houver espaço para colocar na regiao de espera ou o timeOut chegou ao fim 
                if (waitTimeout == 0 || !region.AddWaiter(Thread.CurrentThread.ManagedThreadId))
                    return false;

                int currentTime = (waitTimeout != Timeout.Infinite) ? Environment.TickCount : -1;

                do
                {
                    // caso o tempo de espera chegue ao limite, remove da lista
                    if (Environment.TickCount - currentTime >= waitTimeout && currentTime != -1)
                    {
                        region.RemoveWaiter(Thread.CurrentThread.ManagedThreadId);
                        return false;
                    }
                    
                    try
                    {
                        // fica à esperar
                        Monitor.Wait(lockObj, waitTimeout); 
                    }
                    catch (ThreadInterruptedException)
                    {
                        region.RemoveWaiter(Thread.CurrentThread.ManagedThreadId);
                        // caso haja execption mas permite mais entradas, avisa a todas as threads
                        if (region.permits > 0) 
                            Monitor.PulseAll(lockObj);
                        throw;
                    }

                    // se nao for o 1o continua à espera
                    if ( ! region.IsFirst(Thread.CurrentThread.ManagedThreadId)) 
                        continue;
                    
                    // se tiver permissoes, remove da lista de wait e decrementa as permissoes 
                    if (region.permits > 0 ) 
                    {
                        region.RemoveWaiter(Thread.CurrentThread.ManagedThreadId);
                        region.permits--;
                        return true;
                    }
                    
                   
                } while (true);
            }
        }
        public void Leave(int key)
        {
            MyRegion region;
            lock (lockObj)
            {
                if (!map.ContainsKey(key))
                    throw new Exception("Doesn't exist any Region with this key");

                region = map[key];
            }

            int m;
            int per;
            do
            {
                m = maxInside;
                per = region.permits;
                if(per < m)
                {
                    if (Interlocked.CompareExchange(ref region.permits, per+1, per) > per)
                        break;
                }
                else
                    throw new Exception("The Region doesn't permits more threads");
            } while (true);

            lock (lockObj)
            {
                Monitor.PulseAll(lockObj);
                return;
            }
        }
    }
}
        
