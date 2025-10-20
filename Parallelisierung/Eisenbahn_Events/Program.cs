using System;
using System.Text;
using System.Threading;

class Program
{
    public static readonly int SectionCount = 10;
    public static readonly int SectionWidth = 8;
    public static readonly int TrackLength = SectionCount * SectionWidth;

    static object consoleLock = new object();
    static Random rand = new Random();
    static int trainCounter = 0;
    static Section[] sections = new Section[SectionCount];
    static char[] trackLine = new char[TrackLength];

    static void Main()
    {
        for (int i = 0; i < SectionCount; i++)
        {
            sections[i] = new Section(i);
            sections[i].SectionChanged += OnSectionChanged;
        }

        for (int i = 0; i < TrackLength; i++)
            trackLine[i] = '=';

        Console.Clear();
        Console.CursorVisible = false;

        DrawTrack();
        DrawSections();

        new Thread(InputThread) { IsBackground = true }.Start();

        while (true) Thread.Sleep(500);
    }

    static void InputThread()
    {
        while (true)
        {
            if (Console.ReadKey(true).Key == ConsoleKey.Spacebar)
            {
                int len = rand.Next(2, 6);
                int id = Interlocked.Increment(ref trainCounter);

                var train = new Train(id, len, sections);
                train.TrainMoved += OnTrainMoved;
                train.TrainStatusChanged += OnTrainStatusChanged;

                new Thread(train.Run) { IsBackground = true }.Start();
            }
        }
    }

    static void DrawTrack()
    {
        lock (consoleLock)
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(new string(trackLine));
        }
    }

    static void DrawSections()
    {
        lock (consoleLock)
        {
            var line1 = new StringBuilder();
            var line2 = new StringBuilder();

            for (int i = 0; i < SectionCount; i++)
            {
                line1.Append('|'.ToString().PadRight(SectionWidth));
                line2.Append('/'.ToString().PadRight(SectionWidth));
            }

            Console.SetCursorPosition(0, 1);
            Console.Write(line1.ToString());
            Console.SetCursorPosition(0, 2);
            Console.Write(line2.ToString());
        }
    }

    static void OnTrainMoved(object? sender, TrainMovedEventArgs e)
    {
        lock (consoleLock)
        {
            if (e.NewPosition >= 0 && e.NewPosition < TrackLength)
                trackLine[e.NewPosition] = '|';

            int tail = e.NewPosition - e.Train.Length;
            if (tail >= 0 && tail < TrackLength)
                trackLine[tail] = '=';

            Console.SetCursorPosition(0, 0);
            Console.Write(new string(trackLine));
        }
    }

    static void OnTrainStatusChanged(object? sender, TrainStatusEventArgs e)
    {
        lock (consoleLock)
        {
            int line = 4 + e.Train.Id;
            if (line < Console.WindowHeight)
            {
                Console.SetCursorPosition(0, line);
                Console.Write(e.Status.PadRight(Console.WindowWidth));
            }
        }
    }

    static void OnSectionChanged(object? sender, SectionChangedEventArgs e)
    {
        lock (consoleLock)
        {
            int start = e.Section.Id * SectionWidth;
            char symbol = e.Occupied ? '-' : '/';
            Console.SetCursorPosition(start, 2);
            Console.Write(symbol);
        }
    }

    class Train
    {
        public int Id { get; }
        public int Length { get; }
        public int Position { get; private set; } = -1;
        private Section[] sections;

        public event EventHandler<TrainMovedEventArgs>? TrainMoved;
        public event EventHandler<TrainStatusEventArgs>? TrainStatusChanged;

        public Train(int id, int len, Section[] sections)
        {
            Id = id;
            Length = len;
            this.sections = sections;
        }

        public void Run()
        {
            while (true)
            {
                int next = Position + 1;
                if (next >= TrackLength) break;

                int nextSection = next / SectionWidth;

                if (Position < 0 || (Position / SectionWidth) != nextSection)
                {
                    sections[nextSection].Enter(Id);
                    TrainStatusChanged?.Invoke(this,
                        new TrainStatusEventArgs(this, $"Train {Id}({Length}) entering section {nextSection + 1}"));
                }

                Position = next;
                TrainMoved?.Invoke(this, new TrainMovedEventArgs(this, Position));

                Thread.Sleep(150);

                int tail = Position - Length;
                if (tail >= 0)
                {
                    int tailSection = tail / SectionWidth;
                    if (tailSection != nextSection && (tail + 1) % SectionWidth == 0)
                    {
                        sections[tailSection].Leave(Id);
                        TrainStatusChanged?.Invoke(this,
                            new TrainStatusEventArgs(this, $"Train {Id}({Length}) leaving section {tailSection + 1}"));
                    }
                }
            }

            int lastTail = Position - Length;
            if (lastTail >= 0)
            {
                int s = lastTail / SectionWidth;
                sections[s].Leave(Id);
            }

            TrainStatusChanged?.Invoke(this,
                new TrainStatusEventArgs(this, $"Train {Id}({Length}) finished"));
        }
    }

    class Section
    {
        public int Id { get; }
        private SemaphoreSlim sem;
        public event EventHandler<SectionChangedEventArgs>? SectionChanged;

        public Section(int id)
        {
            Id = id;
            sem = new SemaphoreSlim(1, 1);
        }

        public void Enter(int trainId)
        {
            sem.Wait();
            SectionChanged?.Invoke(this, new SectionChangedEventArgs(this, trainId, true));
        }

        public void Leave(int trainId)
        {
            sem.Release();
            SectionChanged?.Invoke(this, new SectionChangedEventArgs(this, trainId, false));
        }
    }

    class TrainMovedEventArgs : EventArgs
    {
        public Train Train { get; }
        public int NewPosition { get; }
        public TrainMovedEventArgs(Train train, int pos) { Train = train; NewPosition = pos; }
    }

    class TrainStatusEventArgs : EventArgs
    {
        public Train Train { get; }
        public string Status { get; }
        public TrainStatusEventArgs(Train train, string status) { Train = train; Status = status; }
    }

    class SectionChangedEventArgs : EventArgs
    {
        public Section Section { get; }
        public int TrainId { get; }
        public bool Occupied { get; }
        public SectionChangedEventArgs(Section section, int trainId, bool occupied)
        {
            Section = section; TrainId = trainId; Occupied = occupied;
        }
    }
}
