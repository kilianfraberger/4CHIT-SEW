using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

class Program
{
    static readonly int SectionCount = 10;
    static readonly int SectionWidth = 8;
    static SemaphoreSlim[] sectionLocks = new SemaphoreSlim[SectionCount];
    static List<Train> trains = new List<Train>();
    static object trainsLock = new object();
    static object consoleLock = new object();
    static Random rand = new Random();

    static int trainCounter = 0;
    static Dictionary<int, string> trainStatus = new Dictionary<int, string>();

    static void Main()
    {
        for (int i = 0; i < SectionCount; i++)
            sectionLocks[i] = new SemaphoreSlim(1, 1);

        new Thread(InputThread) { IsBackground = true }.Start();

        while (true)
        {
            lock (consoleLock)
            {
                Console.Clear();
                DrawTrack();
                DrawSections();
                DrawTrainStatus();
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
                lock (trainsLock) trains.Add(train);
                lock (trainStatus) trainStatus[id] = $"Train {id}({len}) waiting to start";
                new Thread(train.Run) { IsBackground = true }.Start();
            }
        }
    }

    static void DrawTrack()
    {
        StringBuilder track = new StringBuilder();

        for (int i = 0; i < SectionCount * SectionWidth; i++)
            track.Append("=");

        lock (trainsLock)
        {
            foreach (var t in trains)
            {
                for (int i = 0; i < t.Length; i++)
                {
                    int pos = t.Position - i;
                    if (pos >= 0 && pos < track.Length)
                        track[pos] = '|';
                }
            }
        }

        Console.WriteLine(track.ToString());
    }

    static void DrawSections()
    {
        StringBuilder line1 = new StringBuilder();
        StringBuilder line2 = new StringBuilder();

        for (int i = 0; i < SectionCount; i++)
        {
            line1.Append("|".PadRight(SectionWidth, ' '));
            if (sectionLocks[i].CurrentCount == 0)
                line2.Append("-".PadRight(SectionWidth, ' '));
            else
                line2.Append("/".PadRight(SectionWidth, ' '));
        }

        Console.WriteLine(line1.ToString());
        Console.WriteLine(line2.ToString());
    }

    static void DrawTrainStatus()
    {
        lock (trainStatus)
        {
            foreach (var status in trainStatus.Values)
                Console.WriteLine(status);
        }
    }

    static void UpdateTrainStatus(int id, string status)
    {
        lock (trainStatus)
        {
            trainStatus[id] = status;
        }
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
                    Program.UpdateTrainStatus(Id, $"Train {Id}({Length}) entering section {nextSection + 1}");
                }

                Position = next;
                Thread.Sleep(200);

                int tail = Position - Length;
                if (tail >= 0)
                {
                    int tailSection = tail / SectionWidth;
                    if (tailSection != nextSection && (tail + 1) % SectionWidth == 0)
                    {
                        sectionLocks[tailSection].Release();
                        Program.UpdateTrainStatus(Id, $"Train {Id}({Length}) releasing section {tailSection + 1}");
                    }
                }
            }

            int lastTail = Position - Length;
            if (lastTail >= 0)
            {
                int s = lastTail / SectionWidth;
                sectionLocks[s].Release();
            }

            Program.UpdateTrainStatus(Id, $"Train {Id}({Length}) finished");
            lock (trainsLock) trains.Remove(this);
        }
    }
}
