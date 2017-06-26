using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Serie1Pc
{
    public class RwSemaphore
    {
        private object lockObj = new object();
        private LinkedList<MyObj> lstWaiting = new LinkedList<MyObj>();
        private LinkedList<MyObj> lstInside = new LinkedList<MyObj>();
        public void DownRead()
        {
            lock (lockObj) {

                if ( lstWaiting.Count == 0 && lstInside.Count == 0 )
                {
                    lstInside.AddLast(new MyObj("R"));
                    return;
                }
                if(lstWaiting.Count == 0 && lstInside.First.Value.Desc == "W" )
                {
                    MyObj current = new MyObj("R");

                    LinkedListNode<MyObj> node = lstWaiting.AddLast(current);

                    do
                    {
                        try
                        {
                            Monitor.Wait(lockObj);
                        }
                        catch (ThreadInterruptedException)
                        {
                            lstWaiting.Remove(current);
                            throw;
                        }

                        if (lstInside.Count == 0)
                        {
                            while (lstWaiting.Count != 0 && lstWaiting.First.Value.Desc != "W")
                            {
                                LinkedListNode<MyObj> aux = lstWaiting.First;
                                lstWaiting.RemoveFirst();
                                lstInside.AddLast(aux);
                                aux.Value.Inside = true;
                            }
                        }
                        if (current.Inside)
                            return;

                    } while (true);
                }

                lstWaiting.AddLast(new MyObj("R"));
                try
                {
                    Monitor.Wait(lockObj);
                }
                catch (Exception)
                {
                    throw;
                }
                

            }
        } 
        public void DownWrite()
        {
            lock (lockObj)
            {
                if (lstWaiting.Count == 0 && lstInside.Count == 0)
                {
                    lstInside.AddLast(new MyObj("W"));
                    return;
                }
                MyObj current = new MyObj("W");
                lstWaiting.AddLast(current);
                do
                {
                    try
                    {
                        Monitor.Wait(lockObj);
                    }
                    catch (ThreadInterruptedException)
                    {
                        lstWaiting.Remove(current);
                        if (lstInside.Count == 0)
                            Monitor.PulseAll(lockObj);
                        throw;
                    }
                    if (lstWaiting.First.Value == current && lstInside.Count == 0)
                    {
                        lstInside.AddLast(current);
                        lstWaiting.RemoveFirst();
                        return;
                    }

                } while (true);
            }
        } 
        public void UpRead()
        {
            lock (lockObj)
            {
                foreach (MyObj ob in lstInside)
                {
                    if (ob.Id == Thread.CurrentThread.ManagedThreadId)
                    {
                        lstInside.Remove(ob);

                        if (lstInside.Count == 0)
                            Monitor.PulseAll(lockObj);

                        return;
                    }
                }
                throw new InvalidOperationException();
            }
        }
        public void UpWrite()
        {
            lock (lockObj)
            {
                if (lstInside.Count == 1 && lstInside.First.Value.Id == Thread.CurrentThread.ManagedThreadId)
                {
                    lstInside.RemoveFirst();
                    Monitor.PulseAll(lockObj);
                    return;
                }
                throw new InvalidOperationException();
            }
        }
        public void DowngradeWriter()
        {
            lock (lockObj)
            {
                if (lstInside.Count == 1 && lstInside.First.Value.Id == Thread.CurrentThread.ManagedThreadId)
                {
                    lstInside.First.Value.Desc = "R";

                    while (lstWaiting.Count != 0 && lstWaiting.First.Value.Desc != "W")
                    {
                        LinkedListNode<MyObj> aux = lstWaiting.First;
                        lstWaiting.RemoveFirst();
                        lstInside.AddLast(aux);
                        aux.Value.Inside = true;
                    }
                    Monitor.PulseAll(lockObj);
                    return;
                }
            }
        } 
    }
    public class MyObj
    {
        public bool Inside { get; set; }
        public string Desc { get; set; }
        public int Id { get; set; }
        public MyObj(string desc)
        {
            Desc = desc;
            Id = Thread.CurrentThread.ManagedThreadId;
            Inside = false;
        }
    }
}
