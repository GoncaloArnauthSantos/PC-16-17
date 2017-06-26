using System;
using System.Threading;

namespace Serie1Pc
{
    public class Exchanger<T> where T : class 
    {
        private object lockObj = new object();
        private MyMenssage msg = null;

        public T Exchange(T mine, int timeout)
        {
            lock (lockObj)
            {

                if (msg != null)
                {
                    T toReturn = msg.Msg;
                    msg.Msg = mine;
                    msg = null;
                    Monitor.Pulse(lockObj);
                    return toReturn;
                }

                MyMenssage current = new MyMenssage();
                current.Msg = mine;
                msg = current;

                int currentTime = Environment.TickCount ;

                do
                {
                    try
                    {
                        Monitor.Wait(lockObj, timeout);
                    }
                    catch (ThreadInterruptedException)
                    {
                        msg = null;
                        throw;
                    }

                    if (current.Msg != mine)
                        return current.Msg;

                    if( Environment.TickCount - currentTime >= timeout)
                    {
                        return null;
                    }

                } while (true);
                
            }
        }

        public class MyMenssage
        {
            public T Msg { get; set; }
        }   
    }
    
    
}
