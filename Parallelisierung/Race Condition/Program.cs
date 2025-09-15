using System;
using System.Threading;
using System.Collections.Generic;

class Program
{
    static int x = 0;
    static object locker = new object();

    static void Main()
    {
        int ANZ = 100;
        List<Thread> threads = new List<Thread>();
        
        for (int i = 0; i < ANZ; i++)
        {
            Thread t = new Thread(inc);
            threads.Add(t);
            t.Start();
        }
        
        foreach (var t in threads)
        {
            t.Join();
        }
        Console.WriteLine(x);
    }


    static void inc()
    {
        lock (locker)
        {
            // critical code, critical section
            Thread.Sleep(50);
            x = x + 1;
        }
    }
}