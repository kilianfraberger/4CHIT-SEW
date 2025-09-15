using System;
using System.Threading;

class Program
{
    static void Main()
    {
        const int threadCount = 6;
        const int rounds = 3;
        
        SemaphoreSlim[] semaphores = new SemaphoreSlim[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            semaphores[i] = new SemaphoreSlim(i == 0 ? 1 : 0, 1);
        }
        
        for (int i = 0; i < threadCount; i++)
        {
            int idx = i;
            int next = (i + 1) % threadCount;

            new Thread(() =>
            {
                for (int r = 0; r < rounds; r++)
                {
                    semaphores[idx].Wait();
                    Console.WriteLine($"Spieler {idx + 1}");
                    semaphores[next].Release();
                }
            }).Start();
        }
    }
}