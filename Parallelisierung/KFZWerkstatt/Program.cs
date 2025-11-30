using System;
using System.Threading;

// ---------------- DIAGNOSTIC TECHNICIAN -------------------

public class DiagnosticTech
{
    public string Name { get; set; }
    public SemaphoreSlim DiagnosticTechReady = new SemaphoreSlim(0);
    public SemaphoreSlim DiagnosticDone =  new SemaphoreSlim(0);
    public SemaphoreSlim WaitForCustomer = new SemaphoreSlim(0);


    public DiagnosticTech(string name)
    {
        Name = name;
    }

    public void Run()
    {
        while (true)
        {
            DiagnosticTechReady.Release();
            WaitForCustomer.Wait();
            DoDiagnosis();
            DiagnosticDone.Release();
        }
    }

    private void DoDiagnosis()
    {
        Console.WriteLine($"{Name}: performing computer diagnostics...");
        Thread.Sleep(4000);
        Console.WriteLine($"{Name}: diagnostics complete.");
    }
}

// ---------------- MECHANIC -------------------

public class Mechanic
{
    public SemaphoreSlim MechanicReady = new SemaphoreSlim(0);
    public SemaphoreSlim RepairDone =  new SemaphoreSlim(0);
    public SemaphoreSlim WaitForCustomer = new SemaphoreSlim(0);
    public string Name { get; set; }
    
    public Mechanic(string name)
    {
        Name = name;
    }

    public void Run()
    {
        while (true)
        {
            MechanicReady.Release();
            WaitForCustomer.Wait();
            RepairCar();
            RepairDone.Release();
        }
    }

    private void RepairCar()
    {
        Console.WriteLine($"{Name}: repairing the car...");
        Thread.Sleep(6000);
        Console.WriteLine($"{Name}: repair is done.");
    }
}

// ---------------- CUSTOMER -------------------

public class Customer
{
    public string Name { get; set; }

    private DiagnosticTech tech;
    private Mechanic mechanic;

    public Customer(string name, DiagnosticTech tech, Mechanic mechanic)
    {
        Name = name;
        this.tech = tech;
        this.mechanic = mechanic;
    }

    public void Run()
    {
        Console.WriteLine($"{Name}: arrives at workshop");
        tech.DiagnosticTechReady.Wait();
        Console.WriteLine($"{Name}: going to tech for diagnosis");
        tech.WaitForCustomer.Release();
        tech.DiagnosticDone.Wait();
        mechanic.MechanicReady.Wait();
        Console.WriteLine($"{Name}: going to mechanic for repair");
        mechanic.WaitForCustomer.Release();
        mechanic.RepairDone.Wait();
        Console.WriteLine($"{Name}: picks up the repaired car and leaves");
    }
}

// ---------------- PROGRAM -------------------

class Program
{
    static void Main(string[] args)
    {
        DiagnosticTech tech = new DiagnosticTech("Tech");
        Mechanic[] mechanics = new Mechanic[2];
        mechanics[0] = new Mechanic("MechanicA");
        mechanics[1] = new Mechanic("MechanicB");
        Customer[] customers = new Customer[]
        {
            new Customer("Max",   tech, mechanics[0]),
            new Customer("Lisa",  tech, mechanics[1]),
            new Customer("Tom",   tech, mechanics[0]),
            new Customer("Sarah", tech, mechanics[1]),
            new Customer("Jonas", tech, mechanics[0])
        };
        new Thread(tech.Run).Start();
        foreach (var mechanic in mechanics)
        {
            new Thread(mechanic.Run).Start();
        }
        foreach (Customer customer in customers)
        {
            new Thread(customer.Run).Start();
        }
    }
}
