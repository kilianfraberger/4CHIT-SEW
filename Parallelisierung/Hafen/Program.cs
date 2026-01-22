using System;
using System.Threading;

class Program
{
    static void Main()
    {
        Harbor harbor = new Harbor();
        StagingArea stagingArea = new StagingArea(harbor);

        // 10 Schiffe erzeugen
        for (int i = 1; i <= 10; i++)
        {
            Ship ship = new Ship($"Ship-{i}", harbor);
            new Thread(ship.Run).Start();
        }

        // StagingArea läuft im Hintergrund
        new Thread(stagingArea.Run).Start();

        Console.ReadLine();
    }
}

// --------------------------------------------
// HARBOR – verwaltet die Semaphoren
// --------------------------------------------
public class Harbor
{
    // maximal 5 Schiffe im Hafen
    public SemaphoreSlim EnterHarbor = new SemaphoreSlim(5, 5);

    // 3 Anlegestellen
    public SemaphoreSlim DockingSpots = new SemaphoreSlim(3, 3);

    // StagingArea signalisiert Schiffen -> FIFO
    public SemaphoreSlim WaitingForSignal = new SemaphoreSlim(0);
}

// --------------------------------------------
// STAGING AREA
// --------------------------------------------
public class StagingArea
{
    private Harbor harbor;

    public StagingArea(Harbor harbor)
    {
        this.harbor = harbor;
    }

    public void Run()
    {
        while (true)
        {
            Signal();
            Thread.Sleep(300);  // damit Ausgabe lesbarer ist
        }
    }

    private void Signal()
    {
        // Warte, bis eine Anlegestelle frei ist
        harbor.DockingSpots.Wait();

        Console.WriteLine("StagingArea: signaling a ship to dock");

        // Erlaubt einem wartenden Schiff weiterzumachen
        harbor.WaitingForSignal.Release();
    }
}

// --------------------------------------------
// SHIP
// --------------------------------------------
public class Ship
{
    public string Id { get; set; }
    private Harbor harbor;

    public Ship(string id, Harbor harbor)
    {
        Id = id;
        this.harbor = harbor;
    }

    public void Run()
    {
        // Versuch in den Hafen zu kommen
        harbor.EnterHarbor.Wait();
        Console.WriteLine($"{Id}: entered harbor");

        // Warte bis StagingArea dieses Schiff auswählt
        Console.WriteLine($"{Id}: waiting for docking signal...");
        harbor.WaitingForSignal.Wait();

        Console.WriteLine($"{Id}: received docking signal → docking...");
        Thread.Sleep(400);

        Unload();

        Console.WriteLine($"{Id}: leaving harbor");
        harbor.EnterHarbor.Release();
    }

    private void Unload()
    {
        Thread.Sleep(500);
        Console.WriteLine($"unloading ship: {Id}");

        // Dock wieder freigeben
        harbor.DockingSpots.Release();
    }
}
