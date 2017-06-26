import java.util.ArrayList;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

public class SynchronousThreadPoolExecutor<T>{
    private int maxPoolSize, keepAliveTime, totalThreads;
    private ArrayList<MyObj> submitions = new ArrayList<>();
    private boolean isAlive = true;
    private ArrayList<WorkingThread> wt = new ArrayList<WorkingThread>();
    private final Lock mLock= new ReentrantLock();
    private final Condition resultReady= mLock.newCondition();
    private final Condition workSubmited= mLock.newCondition();
    private final Condition threadEnded= mLock.newCondition();

    public SynchronousThreadPoolExecutor(int maxPoolSize, int keepAliveTime){
        this.maxPoolSize = maxPoolSize;
        this.keepAliveTime = keepAliveTime;
    }
    public T execute(Callable<T> toCall) throws Exception {

        if( !isAlive )
            throw new IllegalStateException();

        mLock.lock();
        WorkingThread current;
        do {
            if (wt.isEmpty()) {
                if (totalThreads < maxPoolSize) {
                    current = new WorkingThread();
                    totalThreads++;
                    break;

                } else {
                    resultReady.await();
                }
            }
            else {
                current = wt.get(0);
                wt.remove(0);
                break;
            }
        }while(true);

        MyObj mO = new MyObj(toCall);
        submitions.add(mO);
        workSubmited.signal();

        while (mO.result == null && mO.excp == null){
            resultReady.await();
        }
        wt.add(current);
        if(mO.result != null) {
            mLock.unlock();
            return mO.result;
        }
        mLock.unlock();
        throw mO.excp;
    }
    public void shutdown()  {
        mLock.lock();
        isAlive = false;
        while(totalThreads != 0){
            try {
                threadEnded.await();
            } catch (InterruptedException e){}
        }
        mLock.unlock();
    }

    private class WorkingThread extends Thread {

        @Override
        public void run() {
            mLock.lock();
            do {
                if (!submitions.isEmpty()) {
                    MyObj sub = submitions.get(0);
                    submitions.remove(0);
                    wt.remove(this);
                    mLock.unlock();
                    try {
                        sub.result = sub.work.call();
                    } catch (Exception e) {
                        sub.excp = e;
                    }
                    mLock.lock();
                    resultReady.signalAll();
                }
                if( isAlive && submitions.isEmpty()) {
                    try {
                        workSubmited.await();
                    } catch (InterruptedException e) {
                        wt.remove(this);
                        totalThreads--;
                        threadEnded.signal();
                        mLock.unlock();
                        return;
                    }
                }
                if( !isAlive && submitions.isEmpty()){
                    wt.remove(this);
                    totalThreads--;
                    threadEnded.signal();
                    mLock.unlock();
                    return;
                }
            }while(true);
        }

    }
    private class MyObj{

        public T result = null;
        public Callable<T> work;
        public Exception excp = null;

        private MyObj( Callable<T> call) {
            this.work = call;
        }
    }
}
    //as defined in the java.util.concurrent package
    interface Callable<V> {
        public V call() throws Exception;
    }

