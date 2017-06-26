
using System.Threading;

namespace Serie2Pc
{
    public class ConcurrentQueue​<T>
    {
        private Node tail, head;
        private static Node dummy;

        public ConcurrentQueue()
        {
            dummy = new Node(default(T));
            head = dummy;
            tail = dummy;
        }

        public void Put(T arg)
        {
            Node myNode = new Node(arg);
            Node t;
            Node tExchange;
            do
            {
                t= tail;
                tExchange = t.Next;

                if( tExchange != null)
                    Interlocked.CompareExchange(ref tail, tExchange, t);

            } while (Interlocked.CompareExchange(ref t.Next, myNode, tExchange) != tExchange );
            Interlocked.CompareExchange(ref tail, myNode, t );
        }
        public T TryTake()
        {
            Node h;
            Node t;
            Node second;
            Node first;
            do
            {
                h = head;
                t = tail;
                first = h.Next;
                
                if (h == t)
                {
                    if( first == null)
                        return default(T);

                    Interlocked.CompareExchange(ref tail, first, t) ;
                }
                second = first.Next;

            } while (Interlocked.CompareExchange(ref h.Next, second, first) != first );

            return first.t;
        }
        public bool IsEmpty()
        {
            Node h = head;
            Node t = tail;

            if (h == t)
                return h.Next == null ;

            return false;
        }

        public class Node
        {
            public T t;
            public Node Next;
            public Node(T arg)
            {
                this.t = arg;
            }
            
        }
    }
   
}
