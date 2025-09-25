using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

class Program
{
    static readonly int SectionCount = 10;
    static readonly int SectionWidth = 8;
    static SemaphoreSlim[] sectionLocks = new SemaphoreSlim[SectionCount];
    static object consoleLock = new object();
    static Random rand = new Random();

    static int trainCounter = 0;
    static Dictionary<int, string> trainStatus = new Dictionary<int, string>();

    static void Main()
    {
        for (int i = 0; i < SectionCount; i++)
            sectionLocks[i] = new SemaphoreSlim(1, 1);


        Console.SetCursorPosition(0, 0);
        Console.WriteLine(new string('=', SectionCount * SectionWidth));

        new Thread(InputThread) { IsBackground = true }.Start();

        Console.CursorVisible = false;

        while (true)
        {
            lock (consoleLock)
            {
                int line = 2;
                Console.SetCursorPosition(0, line++);
                WriteLineFixed(DrawSectionsLine1());

                Console.SetCursorPosition(0, line++);
                WriteLineFixed(DrawSectionsLine2());

                lock (trainStatus)
                {
                    foreach (var status in trainStatus.Values)
                    {
                        Console.SetCursorPosition(0, line++);
                        WriteLineFixed(status);
                    }
                } 
            }

            Thread.Sleep(150);
        }
    }


    static void InputThread()
    {
        while (true)
        {
            if (Console.ReadKey(true).Key == ConsoleKey.Spacebar)
            {
                int len = rand.Next(1, 7);
                int id = Interlocked.Increment(ref trainCounter);
                var train = new Train(id, len);
                lock (trainStatus) trainStatus[id] = $"Train {id}({len}) waiting to start";
                new Thread(train.Run) { IsBackground = true }.Start();
            }
        }
    }



    static string DrawSectionsLine1()
    {
        StringBuilder line1 = new StringBuilder();
        for (int i = 0; i < SectionCount; i++)
            line1.Append("|".PadRight(SectionWidth, ' '));
        return line1.ToString();
    }

    static string DrawSectionsLine2()
    {
        StringBuilder line2 = new StringBuilder();
        for (int i = 0; i < SectionCount; i++)
        {
            if (sectionLocks[i].CurrentCount == 0)
                line2.Append("-".PadRight(SectionWidth, ' '));
            else
                line2.Append("/".PadRight(SectionWidth, ' '));
        }

        return line2.ToString();
    }

    static void UpdateTrainStatus(int id, string status)
    {
        lock (trainStatus)
        {
            trainStatus[id] = status;
        }
    }

    static void WriteLineFixed(string text)
    {
        Console.Write(text.PadRight(Console.WindowWidth));
    }

    class Train
    {
        public int Id { get; }
        public int Length { get; }
        public int Position { get; private set; } = -1;

        public Train(int id, int len)
        {
            Id = id;
            Length = len;
        }

        public void Run()
        {
            while (true)
            {
                int next = Position + 1;
                if (next >= SectionCount * SectionWidth) break;

                int nextSection = next / SectionWidth;

                if (Position < 0 || (Position / SectionWidth) != nextSection)
                {
                    sectionLocks[nextSection].Wait();
                    UpdateTrainStatus(Id, $"Train {Id}({Length}) entering section {nextSection + 1}");
                }

                Position = next;
                
                lock (consoleLock)
                {
                    Console.SetCursorPosition(Position, 0);
                    Console.Write("|");
                }
                
                int tail = Position - Length;
                if (tail >= 0)
                {
                    lock (consoleLock)
                    {
                        Console.SetCursorPosition(tail, 0);
                        Console.Write("=");
                    }
                }

                Thread.Sleep(200);

                if (tail >= 0)
                {
                    int tailSection = tail / SectionWidth;
                    if (tailSection != nextSection && (tail + 1) % SectionWidth == 0)
                    {
                        sectionLocks[tailSection].Release();
                        UpdateTrainStatus(Id, $"Train {Id}({Length}) releasing section {tailSection + 1}");
                    }
                }
            }

            int lastTail = Position - Length;
            if (lastTail >= 0)
            {
                int s = lastTail / SectionWidth;
                sectionLocks[s].Release();
            }

            UpdateTrainStatus(Id, $"Train {Id}({Length}) finished");
        }
    }
}
