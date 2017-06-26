using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Serie1Pc
{
    public class RetryLazy<T> where T : class
    {
        private Func<T> provider;
        private int maxRetries;
        private object lockObj = new object();
        private LinkedList<int> lst = new LinkedList<int>();
        private T current = null;

        public RetryLazy(Func<T> provider, int maxRetries)
        {
            this.provider = provider;
            this.maxRetries = maxRetries;
        }
        public T Value { get {

                lock (lockObj)
                {
                    if (current != null)
                        return current;

                    if (maxRetries == 0)
                        throw new InvalidOperationException();

                    LinkedListNode<int> myNode = lst.AddLast(Thread.CurrentThread.ManagedThreadId);
                    do
                    {
                        if (lst.First == myNode)
                        {
                            T aux = null;
                            try
                            {
                                Monitor.Exit(lockObj);
                                aux = provider();
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            finally
                            {
                                Monitor.Enter(lockObj);
                                current = aux;
                                maxRetries--;
                                lst.RemoveFirst();
                                Monitor.PulseAll(lockObj);
                            }
                            return current;
                        }

                        try
                        {
                            Monitor.Wait(lockObj);
                        }
                        catch (ThreadInterruptedException)
                        {
                            lst.Remove(Thread.CurrentThread.ManagedThreadId);
                            throw;
                        }

                        if (current != null)
                        {
                            lst.Remove(Thread.CurrentThread.ManagedThreadId);
                            return current;
                        }
                        if (maxRetries == 0)
                            throw new InvalidOperationException();

                    } while (true);
                }
            }
        } 
    }
}
