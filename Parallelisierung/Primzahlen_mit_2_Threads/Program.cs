using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static int max = 1_000_000_000;
    static bool[] isPrime = new bool[max + 1];

    static void Main()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        for (int i = 2; i <= max; i++)
            isPrime[i] = true;

        Thread t1 = new Thread(SieveRange1);
        Thread t2 = new Thread(SieveRange2);

        t1.Start();
        t2.Start();

        t1.Join();
        t2.Join();

        sw.Stop();
        Console.WriteLine($"Berechnung fertig in {sw.ElapsedMilliseconds} ms.");
    }
    
    static void SieveRange1()
    {
        SieveRange(2, 500_000_000);
    }

    static void SieveRange2()
    {
        SieveRange(500_000_001, max);
    }

    static void SieveRange(int start, int end)
    {
        int limit = (int)Math.Sqrt(end);

        for (int i = 2; i <= limit; i++)
        {
            if (isPrime[i])
            {
                int begin = Math.Max(i * i, ((start + i - 1) / i) * i);
                for (int j = begin; j <= end; j += i)
                {
                    isPrime[j] = false;
                }
            }
        }
    }
}