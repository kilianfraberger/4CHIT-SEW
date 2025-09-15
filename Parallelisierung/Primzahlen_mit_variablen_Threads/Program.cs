using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static int max = 1_000_000_000;
    static bool[] isPrime = new bool[max + 1];

    static void Main()
    {
        int maxThreads = Environment.ProcessorCount;
        Console.Write("Wie viele Threads verwenden? (1-"+maxThreads+"): ");
        int threadCount;
        if (!int.TryParse(Console.ReadLine(), out threadCount) || threadCount < 1)
            threadCount = 1;
        
        if (threadCount > maxThreads)
            threadCount = maxThreads;

        Stopwatch sw = new Stopwatch();
        sw.Start();

        for (int i = 2; i <= max; i++)
            isPrime[i] = true;

        Thread[] threads = new Thread[threadCount];

        int rangeSize = max / threadCount;

        for (int t = 0; t < threadCount; t++)
        {
            int start = t * rangeSize + 1;
            int end = (t == threadCount - 1) ? max : (t + 1) * rangeSize;

            int localStart = start;
            int localEnd = end;

            threads[t] = new Thread(() => SieveRange(localStart, localEnd));
            threads[t].Start();
        }

        foreach (var thread in threads)
            thread.Join();

        sw.Stop();
        Console.WriteLine($"Berechnung fertig in {sw.ElapsedMilliseconds} ms mit {threadCount} Threads.");
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