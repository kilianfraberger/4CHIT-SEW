using System;
using System.Threading;

public class Cook
{
    public string Name { get; set; }

    public SemaphoreSlim KochFrei;         // Koch ist frei
    public SemaphoreSlim BestellungFertig;         // Kunde gibt Bestellung
    public SemaphoreSlim EssenFertig;     // Essen ist fertig

    public Cook(string name, SemaphoreSlim KochFrei, SemaphoreSlim BestellungFertig, SemaphoreSlim EssenFertig)
    {
        Name = name;
        this.KochFrei = KochFrei; 
        this.BestellungFertig= BestellungFertig;
        this.EssenFertig = EssenFertig;
    }

    public void Run()
    {
        while (true)
        {
            KochFrei.Release();            // (1) Ich bin bereit
            BestellungFertig.Wait();               // (2) Warten auf Bestellung
            PrepareMeal();
            EssenFertig.Release();        // (3) Essen fertig
            Thread.Sleep(1000);
        }
    }

    private void PrepareMeal()
    {
        Console.WriteLine($"{Name}: cooking food...");
        Thread.Sleep(10000);
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
            Thread.Sleep(1000);
        }
    }

    public void Confirm()
    {
        Console.WriteLine($"{Name}: confirming payment...");
        Thread.Sleep(5000);
        Console.WriteLine($"{Name}: Gave receipt");
    }
}

// ---------------- CUSTOMER -------------------

public class Customer
{
    public SemaphoreSlim KochFrei;         // Koch ist frei
    public SemaphoreSlim BestellungFertig;         // Kunde gibt Bestellung
    public SemaphoreSlim EssenFertig;   
    public string Name { get; set; }
    private Cashier cashier;

    public Customer(string name, Cashier cashier, SemaphoreSlim KochFrei, SemaphoreSlim BestellungFertig, SemaphoreSlim EssenFertig)
    {
        Name = name;
        this.cashier = cashier;
        this.KochFrei = KochFrei; 
        this.BestellungFertig= BestellungFertig;
        this.EssenFertig = EssenFertig;
    }

    public void Run()
    {
        Console.WriteLine($"{Name}: wants to order");

        // (1) warte auf freien Koch
        KochFrei.Wait();

        // (2) gib Warteplatz frei → Bestellung abgeben
        Console.WriteLine($"{Name}: ordered");
        BestellungFertig.Release();
        // (3) warte auf Essen
        EssenFertig.Wait();

        Console.WriteLine($"{Name}: got food!");
        Console.WriteLine($"{Name}: Goes to cashier");
        Thread.Sleep(5000);
        Console.WriteLine($"{Name}: Wants to pay");
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
        SemaphoreSlim KochFrei = new SemaphoreSlim(0);
        SemaphoreSlim BestellungFertig = new SemaphoreSlim(0);
        SemaphoreSlim EssenFertig = new SemaphoreSlim(0);
        Cook cook1 = new Cook("Abdul", KochFrei, BestellungFertig, EssenFertig);
        Cook cook2 = new Cook("Hakan", KochFrei, BestellungFertig, EssenFertig);

        Cashier cashier = new Cashier("Ali");

        // Start worker threads
        new Thread(cook1.Run).Start();
        new Thread(cook2.Run).Start();
        new Thread(cashier.Run).Start();

        // Kunden
        var customers = new Customer[]
        {
            new Customer("Max", cashier, KochFrei, BestellungFertig, EssenFertig),
            new Customer("Lisa", cashier, KochFrei, BestellungFertig, EssenFertig),
            new Customer("Tom", cashier, KochFrei, BestellungFertig, EssenFertig),
            new Customer("Anna", cashier, KochFrei, BestellungFertig, EssenFertig),
        };

        foreach (var c in customers)
            new Thread(c.Run).Start();
    }
}



//Kassier 1. bin bereit 2. Warte
//Koch 1. bereit 2. Warte auf Bestellung 3. gib Speise frei
//Kunde: 1. warte auf Koch 2. gib warteplatz frei 3. warte auf Essen 4. warte auf kassa 5. gib kassaplatz frei