// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

public class machineA
{
    private ConveyerBelt conveyerBelt;

    public machineA(ConveyerBelt conveyerBelt)
    {
        this.conveyerBelt = conveyerBelt;
    }
    public void Run()
    {
        while (true)
        {
            conveyerBelt.machineAStart.Wait();
            Process();
            conveyerBelt.machineADone.Release();
        }
    }

    private void Process()
    {
        Thread.Sleep(5000);
        Console.WriteLine("MachineA: finished process");
    }
}

public class machineB
{
    private ConveyerBelt conveyerBelt;

    public machineB(ConveyerBelt conveyerBelt)
    {
        this.conveyerBelt = conveyerBelt;
    }
    public void Run()
    {
        while (true)
        {
            conveyerBelt.machineBStart.Wait();
            Process();
            conveyerBelt.machineBDone.Release();
        }
    }

    private void Process()
    {
        Thread.Sleep(2000);
        Console.WriteLine("MachineB: finished process");
    }
}

public class ConveyerBelt
{
    public SemaphoreSlim machineADone = new SemaphoreSlim(1);
    public SemaphoreSlim machineBDone = new SemaphoreSlim(1);
    public SemaphoreSlim machineAStart = new SemaphoreSlim(0);
    public SemaphoreSlim machineBStart = new SemaphoreSlim(0);

    private bool lagerWerkStück = false;
    private bool machineAWerkStück = false;
    private bool machineBWerkStück = false;
    Random rand = new Random();
    public void Run()
    {
        while (true)
        {
            machineADone.Wait();
            machineBDone.Wait();
            move();
            if(machineAWerkStück)
                machineAStart.Release(); 
            else
                machineADone.Release();
            if(machineBWerkStück)
                machineBStart.Release();
            else
                machineBDone.Release();
        }
    }

    public void move()
    {
        Thread.Sleep(3000);
        machineBWerkStück = machineAWerkStück;
        machineAWerkStück = lagerWerkStück;
        lagerWerkStück = rand.Next(2) == 0;
        Console.WriteLine($"Moved Workpieces: {lagerWerkStück} -> {machineAWerkStück} -> {machineBWerkStück}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        ConveyerBelt c = new ConveyerBelt();
        machineA a = new machineA(c);
        machineB b = new machineB(c);
        
        new Thread(c.Run).Start();
        new Thread(a.Run).Start();
        new Thread(b.Run).Start();
    }
}