using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

class Program
{
    public static readonly int SectionCount = 10;
    public static readonly int SectionWidth = 8;
    public static object consoleLock = new object();
    static Random rand = new Random();

    static int trainCounter = 0;
    static Section[] sections = new Section[SectionCount];

    static void Main()
    {
        for (int i = 0; i < SectionCount; i++)
        {
            sections[i] = new Section(i);
            sections[i].AddObserver(new SectionObserver());
        }
        
        Console.SetCursorPosition(0, 0);
        Console.WriteLine(new string('=', SectionCount * SectionWidth));
        
        DrawAllSections();

        Console.CursorVisible = false;
        
        new Thread(InputThread) { IsBackground = true }.Start();
        
        while (true)
        {
            Thread.Sleep(200);
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

                var train = new Train(id, len, sections);
                var observer = new TrainObserver(train);

                new Thread(train.Run) { IsBackground = true }.Start();
            }
        }
    }

    static void DrawAllSections()
    {
        lock (consoleLock)
        {
            StringBuilder line1 = new StringBuilder(new string(' ', SectionCount * SectionWidth));
            StringBuilder line2 = new StringBuilder(new string(' ', SectionCount * SectionWidth));

            for (int i = 0; i < SectionCount; i++)
            {
                int start = i * SectionWidth;
                int end = start + SectionWidth - 1;

                line1[start] = '|';
                line2[start] = '/';

            }

            Console.SetCursorPosition(0, 1);
            Console.WriteLine(line1.ToString());
            Console.SetCursorPosition(0, 2);
            Console.WriteLine(line2.ToString());
        }
    }
    
    class Train
    {
        public int Id { get; }
        public int Length { get; }
        public int Position { get; private set; } = -1;

        private List<TrainObserver> observers = new List<TrainObserver>();
        private Section[] sections;

        public Train(int id, int len, Section[] sections)
        {
            Id = id;
            Length = len;
            this.sections = sections;
        }

        public void AddObserver(TrainObserver observer)
        {
            observers.Add(observer);
        }

        private void NotifyMove(int newPos)
        {
            foreach (var obs in observers)
                obs.OnTrainMoved(this, newPos);
        }

        private void NotifyStatus(string status)
        {
            foreach (var obs in observers)
                obs.OnTrainStatusChanged(this, status);
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
                    sections[nextSection].Enter(Id);
                    NotifyStatus($"Train {Id}({Length}) entering section {nextSection + 1}");
                }
                
                Position = next;
                NotifyMove(Position);

                Thread.Sleep(200);

                int tail = Position - Length;
                if (tail >= 0)
                {
                    int tailSection = tail / SectionWidth;
                    if (tailSection != nextSection && (tail + 1) % SectionWidth == 0)
                    {
                        sections[tailSection].Leave(Id);
                        NotifyStatus($"Train {Id}({Length}) releasing section {tailSection + 1}");
                    }
                }
            }

            int lastTail = Position - Length;
            if (lastTail >= 0)
            {
                int s = lastTail / SectionWidth;
                sections[s].Leave(Id);
            }

            NotifyStatus($"Train {Id}({Length}) finished");
        }
    }

    class TrainObserver
    {
        private Train train;

        public TrainObserver(Train t)
        {
            train = t;
            train.AddObserver(this);
        }

        public void OnTrainMoved(Train train, int newPosition)
        {
            lock (consoleLock)
            {
                Console.SetCursorPosition(newPosition, 0);
                Console.Write("|");
                
                int tail = newPosition - train.Length;
                if (tail >= 0)
                {
                    Console.SetCursorPosition(tail, 0);
                    Console.Write("=");
                }
            }
        }

        public void OnTrainStatusChanged(Train train, string status)
        {
            lock (consoleLock)
            {
                int line = 4 + train.Id;
                if (line < Console.WindowHeight)
                {
                    Console.SetCursorPosition(0, line);
                    Console.Write(status.PadRight(Console.WindowWidth));
                }
            }
        }
    }
    

    class Section
    {
        public int Id { get; }
        private SemaphoreSlim sem;
        private List<SectionObserver> observers = new List<SectionObserver>();

        public Section(int id)
        {
            Id = id;
            sem = new SemaphoreSlim(1, 1);
        }

        public void AddObserver(SectionObserver observer) => observers.Add(observer);

        public void Enter(int trainId)
        {
            sem.Wait();
            NotifyStatus(trainId, true);
        }

        public void Leave(int trainId)
        {
            sem.Release();
            NotifyStatus(trainId, false);
        }

        private void NotifyStatus(int trainId, bool occupied)
        {
            foreach (var obs in observers)
                obs.OnSectionChanged(this, trainId, occupied);
        }
    }

    class SectionObserver
    {
        public void OnSectionChanged(Section section, int trainId, bool occupied)
        {
            lock (consoleLock)
            {
                int line1 = 1;
                int line2 = 2;
                int start = section.Id * SectionWidth;
                int end = start + SectionWidth - 1;
                
                Console.SetCursorPosition(start, line1);
                Console.Write("|");
                
                char symbol = occupied ? '-' : '/';
                Console.SetCursorPosition(start, line2);
                Console.Write(symbol);
            }
        }
    }
}
