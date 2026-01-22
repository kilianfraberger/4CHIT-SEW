using System;
using System.Threading;

class Program
{
    static void Main()
    {
        // Züge starten
        for (int i = 0; i < 1; i++)
        {
            new Thread(new TrainA().Run).Start();
            new Thread(new TrainB().Run).Start();
        }

        Console.ReadLine();
    }
}

// --------------------------------------------------
// INFRASTRUKTUR: BAHNHÖFE + GLEISE
// --------------------------------------------------
public static class TrainSystem
{
    // Bahnsteige
    public static SemaphoreSlim KyotoPlatforms = new(7, 7);
    public static SemaphoreSlim ShizuokaPlatforms = new(5, 5);
    public static SemaphoreSlim TokyoPlatforms = new(11, 11);

    // Strecken (Gleise)
    public static SemaphoreSlim Track_Kyoto_Shizuoka = new(3, 3);
    public static SemaphoreSlim Track_Shizuoka_Tokyo = new(5, 5);
}

// --------------------------------------------------
// BASISKLASSE ATrain
// --------------------------------------------------
public abstract class ATrain
{
    public static int indexer = 0;
    public static SemaphoreSlim idLock = new(1, 1);

    protected int _id;

    public ATrain()
    {
        idLock.Wait();
        _id = ++indexer;
        idLock.Release();
    }

    // --------------------------- Kyoto -------------------------------
    public void EnterKyoto()
    {
        TrainSystem.KyotoPlatforms.Wait();
        Console.WriteLine($"Train {_id}: Entering Kyoto");
        Thread.Sleep(500);
        TrainSystem.KyotoPlatforms.Release();
    }

    // --------------------------- Shizuoka ----------------------------
    public void EnterShizuoka()
    {
        TrainSystem.ShizuokaPlatforms.Wait();
        Console.WriteLine($"Train {_id}: Entering Shizuoka");
        Thread.Sleep(300);
        TrainSystem.ShizuokaPlatforms.Release();
    }

    // --------------------------- Tokyo -------------------------------
    public void EnterTokyo()
    {
        TrainSystem.TokyoPlatforms.Wait();
        Console.WriteLine($"Train {_id}: Entering Tokyo");
        Thread.Sleep(700);
        TrainSystem.TokyoPlatforms.Release();
    }
}

// --------------------------------------------------
// Train A  -> Kyoto → Shizuoka → Tokyo
// --------------------------------------------------
public class TrainA : ATrain
{
    public void Run()
    {
        // Kyoto betreten
        EnterKyoto();

        // Strecke Kyoto → Shizuoka
        TrainSystem.Track_Kyoto_Shizuoka.Wait();
        Console.WriteLine($"Train {_id}: traveling Kyoto → Shizuoka");
        Thread.Sleep(400);
        EnterShizuoka();
        TrainSystem.Track_Kyoto_Shizuoka.Release();

        // Strecke Shizuoka → Tokyo
        TrainSystem.Track_Shizuoka_Tokyo.Wait();
        Console.WriteLine($"Train {_id}: traveling Shizuoka → Tokyo");
        Thread.Sleep(500);
        EnterTokyo();
        TrainSystem.Track_Shizuoka_Tokyo.Release();
    }
}

// --------------------------------------------------
// Train B  -> Tokyo → Shizuoka → Kyoto
// --------------------------------------------------
public class TrainB : ATrain
{
    public void Run()
    {
        // Tokyo betreten
        EnterTokyo();

        // Strecke Tokyo → Shizuoka
        TrainSystem.Track_Shizuoka_Tokyo.Wait();
        Console.WriteLine($"Train {_id}: traveling Tokyo → Shizuoka");
        Thread.Sleep(500);
        EnterShizuoka();
        TrainSystem.Track_Shizuoka_Tokyo.Release();

        // Strecke Shizuoka → Kyoto
        TrainSystem.Track_Kyoto_Shizuoka.Wait();
        Console.WriteLine($"Train {_id}: traveling Shizuoka → Kyoto");
        Thread.Sleep(400);
        EnterKyoto();
        TrainSystem.Track_Kyoto_Shizuoka.Release();
    }
}
