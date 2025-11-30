// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

public class Schifahrer
{
    private Random rand = new Random();
    private Race r;

    public Schifahrer(Race r)
    {
       this.r = r;
    }
    public void Run()
    {
        Thread.Sleep(rand.Next(2000, 5000));
        Console.WriteLine("Schifahrer ready");
        r.Ready.Release();
        r.RaceStart.Wait();
        Racing();
        r.RacerFinished.Release();
    }

    public void Racing()
    {
        Thread.Sleep(rand.Next(9500, 10000));
    }
}

public class Race
{
    public SemaphoreSlim Ready = new SemaphoreSlim(0);
    public SemaphoreSlim RaceStart = new SemaphoreSlim(0);
    public SemaphoreSlim RacerFinished = new SemaphoreSlim(0);
    private Stopwatch stopwatch=new Stopwatch();
    public void Run()
    {
        for (int i = 0; i < 2; i++)
        {
            Ready.Wait();
        }
        Start();
        RaceStart.Release();
        stopwatch.Start();
        RacerFinished.Wait();
        stopwatch.Stop();
        End();
    }

    public void Start()
    {
        Console.WriteLine("Waiting to Start");
        Thread.Sleep(2000);
        Console.WriteLine("Go! Go! Go!");
    }

    public void End()
    {
        Console.WriteLine("First Racer finished: "+stopwatch.Elapsed);
    }
}

class Program
{
    static void Main(string[] args)
    {
        Race r = new Race();
        Schifahrer[] s= new Schifahrer[2];
        s[0] = new Schifahrer(r);
        s[1] = new Schifahrer(r);
        foreach (var fahrer in s)
        {
            new Thread(fahrer.Run).Start();
        }
        new Thread(r.Run).Start();
    }
}