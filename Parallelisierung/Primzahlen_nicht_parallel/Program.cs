using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        int count = 0;
        int max = 1_000_000_000;
        bool[] isPrime = new bool[max + 1];

        for (int i = 2; i <= max; i++)
            isPrime[i] = true;

        for (int i = 2; i * i <= max; i++)
        {
            if (isPrime[i])
            {
                for (int j = i * i; j <= max; j += i)
                    isPrime[j] = false;
            }
        }
        
        for (int i = 2; i <= max; i++)
        {
            if (isPrime[i])
                count++;
        }

        sw.Stop();
        Console.WriteLine($"Berechnung fertig in {sw.ElapsedMilliseconds} ms.");
        Console.WriteLine($"Anzahl der Primzahlen bis {max}: {count}");
    }
}