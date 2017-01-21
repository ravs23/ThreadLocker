using System;
using System.Threading;

class Locker
{
    int wait;
    int busy;

    public Locker(int wait)
    {
        this.wait = wait;
    }

    public void Enter()
    {
        int result = Interlocked.CompareExchange(ref busy, 1, 0);

        if (result == 1)
            while (busy == 1)
            {
                Thread.Sleep(wait);
            }
        Interlocked.Exchange(ref busy, 1);
    }

    public void Exit()
    {
        Interlocked.Exchange(ref busy, 0);
    }
}

class ManagedLocker : IDisposable
{
    readonly Locker locker;

    public ManagedLocker(Locker locker)
    {
        this.locker = locker;
        locker.Enter();
    }

    public void Dispose()
    {
        locker.Exit();
    }
}

class Program
{
    static int counter;
    static object loc = new object();
    static Locker locker = new Locker(10);

    static void Main(string[] args)
    {
        Thread[] threads = new Thread[10];
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(new ThreadStart(Method));
            threads[i].Start();
        }
    }

    static void Method()
    {

        //  lock (loc)

        using (new ManagedLocker(locker))
        {
            Console.WriteLine("Поток {0} запущен", Thread.CurrentThread.ManagedThreadId);
            int i;
            for (i = 0; i < 5000; i++)
            {
                counter++;
                Thread.Sleep(1);
            }

            Console.WriteLine("Поток {0} завершен, {1}", Thread.CurrentThread.ManagedThreadId, counter);
        }
    }
}