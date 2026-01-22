using System;
using System.Threading;

public class Sentinel
{
    private SemaphoreSlim HarvesterAvailable;
    private SemaphoreSlim Storage;
    private SemaphoreSlim WaitForHarvester;
    private SemaphoreSlim SendCoordinates;
    public string Name { get; }
    private Random rnd = new Random();

    public Sentinel(string name, SemaphoreSlim harvesterAvailable, SemaphoreSlim storage,  SemaphoreSlim waitForHarvester, SemaphoreSlim sendCoordinates)
    {
        Name = name;
        HarvesterAvailable = harvesterAvailable;
        Storage = storage;
        WaitForHarvester = waitForHarvester;
        SendCoordinates = sendCoordinates;
    }

    public void Run()
    {
        while (true)
        {
            ScanningSurface();

            Console.WriteLine($"{Name}: found resources → signaling a harvester. Waiting for Acknowledging");
            HarvesterAvailable.Release();  // gibt einen wartenden Harvester frei
            WaitForHarvester.Wait();
            Console.WriteLine($"{Name}: Acknowledging received. Sending Coordinates");
            SendCoordinates.Release();
        }
    }

    private void ScanningSurface()
    {
        Console.WriteLine($"{Name}: scanning surface...");
        Thread.Sleep(2000);
    }
}

public class Harvester
{
    private SemaphoreSlim HarvesterAvailable; // warten auf Sentinel-Signal
    private SemaphoreSlim Storage;            // max 2 im Lager
    private SemaphoreSlim WaitForHarvester;
    private SemaphoreSlim SendCoordinates;
    public string Name { get; }

    public Harvester(string name, SemaphoreSlim harvesterAvailable, SemaphoreSlim storage,  SemaphoreSlim waitForHarvester, SemaphoreSlim sendCoordinates)
    {
        Name = name;
        HarvesterAvailable = harvesterAvailable;
        Storage = storage;
        WaitForHarvester = waitForHarvester;
        SendCoordinates = sendCoordinates;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine($"{Name}: waiting for sentinel signal...");
            HarvesterAvailable.Wait();   // wartet auf Signal vom Sentinel
            Console.WriteLine($"{Name}: Acknowleding");
            WaitForHarvester.Release();
            SendCoordinates.Wait();
            Console.WriteLine($"{Name}: Received Coordinates");
            Harvest();

            Store();
        }
    }

    private void Harvest()
    {
        Console.WriteLine($"{Name}: harvesting resources...");
        Thread.Sleep(3000);
        Console.WriteLine($"{Name}: harvested resources");
    }

    private void Store()
    {
        Console.WriteLine($"{Name}: heading to storage...");

        Storage.Wait(); // max 2 gleichzeitig
        Console.WriteLine($"{Name}: storing resources...");
        Thread.Sleep(2000);
        Console.WriteLine($"{Name}: leaving storage.");
        Storage.Release();
    }
}

class Program
{
    static void Main()
    {
        int harvesterCount = 2;
        int sentinelCount = 2;

        SemaphoreSlim HarvesterAvailable = new SemaphoreSlim(0);
        SemaphoreSlim Storage = new SemaphoreSlim(2);
        SemaphoreSlim WaitForHarvester = new SemaphoreSlim(0);
        SemaphoreSlim SendCoordinates = new SemaphoreSlim(0);

        Harvester[] harvesters = new Harvester[harvesterCount];
        for (int i = 0; i < harvesterCount; i++)
        {
            harvesters[i] = new Harvester($"Harvester {i + 1}", HarvesterAvailable, Storage, WaitForHarvester, SendCoordinates);
            new Thread(harvesters[i].Run).Start();
        }

        Sentinel[] sentinels = new Sentinel[sentinelCount];
        for (int i = 0; i < sentinelCount; i++)
        {
            sentinels[i] = new Sentinel($"Sentinel {i + 1}", HarvesterAvailable, Storage, WaitForHarvester, SendCoordinates);
            new Thread(sentinels[i].Run).Start();
        }
    }
}
