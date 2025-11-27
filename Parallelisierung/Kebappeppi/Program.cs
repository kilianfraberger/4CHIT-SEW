using System;
using System.Threading;

public class Cook
{
    public string Name { get; set; }

    public SemaphoreSlim KochFrei;         // Koch ist frei
    public SemaphoreSlim BestellungFertig;         // Kunde gibt Bestellung
    public SemaphoreSlim EssenFertig;     // Essen ist fertig

    public Cook(string name)
    {
        Name = name;
        KochFrei = new SemaphoreSlim(1); 
        BestellungFertig = new SemaphoreSlim(0);
        EssenFertig = new SemaphoreSlim(0);
    }

    public void Run()
    {
        while (true)
        {
            KochFrei.Release();            // (1) Ich bin bereit
            BestellungFertig.Wait();               // (2) Warten auf Bestellung
            PrepareMeal();
            EssenFertig.Release();        // (3) Essen fertig
        }
    }

    private void PrepareMeal()
    {
        Console.WriteLine($"{Name}: cooking food...");
        Thread.Sleep(1200);
    }
}

// ---------------- CASHIER -------------------

public class Cashier
{
    public string Name { get; set; }

    public SemaphoreSlim KassaFrei;   // Cashier ist frei
    public SemaphoreSlim GeldDa;  // Kunde möchte zahlen
    public SemaphoreSlim Beleg;

    public Cashier(string name)
    {
        Name = name;
        KassaFrei = new SemaphoreSlim(1);
        GeldDa = new SemaphoreSlim(0);
        Beleg = new SemaphoreSlim(0);
    }

    public void Run()
    {
        while (true)
        {
            KassaFrei.Release();      // (1) Ich bin bereit
            GeldDa.Wait();        // (2) Warten auf Kunden
            Confirm();
            Beleg.Release();
        }
    }

    public void Confirm()
    {
        Console.WriteLine($"{Name}: confirming payment...");
        Thread.Sleep(1000);
        Console.WriteLine($"{Name}: Gave receipt");
    }
}

// ---------------- CUSTOMER -------------------

public class Customer
{
    public string Name { get; set; }
    private Cook cook;
    private Cashier cashier;

    public Customer(string name, Cook cook, Cashier cashier)
    {
        Name = name;
        this.cook = cook;
        this.cashier = cashier;
    }

    public void Run()
    {
        Console.WriteLine($"{Name}: wants to order");

        // (1) warte auf freien Koch
        cook.KochFrei.Wait();

        // (2) gib Warteplatz frei → Bestellung abgeben
        Console.WriteLine($"{Name}: ordered");
        cook.BestellungFertig.Release();
        // (3) warte auf Essen
        cook.EssenFertig.Wait();

        Console.WriteLine($"{Name}: got food!");

        // (4) warte auf freien Cashier
        cashier.KassaFrei.Wait();

        // (5) gib Kassaplatz frei → jetzt zahlen
        cashier.GeldDa.Release();
        cashier.Beleg.Wait();
        Console.WriteLine($"{Name}: finished paying");
    }
}

// ---------------- PROGRAM -------------------

class Program
{
    static void Main(string[] args)
    {
        Cook cook1 = new Cook("Abdul");
        Cook cook2 = new Cook("Hakan");

        Cashier cashier = new Cashier("Ali");

        // Start worker threads
        new Thread(cook1.Run).Start();
        new Thread(cook2.Run).Start();
        new Thread(cashier.Run).Start();

        // Kunden
        var customers = new Customer[]
        {
            new Customer("Max",  cook1, cashier),
            new Customer("Lisa", cook2, cashier),
            new Customer("Tom",  cook1, cashier),
            new Customer("Anna", cook2, cashier),
        };

        foreach (var c in customers)
            new Thread(c.Run).Start();
    }
}



//Kassier 1. bin bereit 2. Warte
//Koch 1. bereit 2. Warte auf Bestellung 3. gib Speise frei
//Kunde: 1. warte auf Koch 2. gib warteplatz frei 3. warte auf Essen 4. warte auf kassa 5. gib kassaplatz frei