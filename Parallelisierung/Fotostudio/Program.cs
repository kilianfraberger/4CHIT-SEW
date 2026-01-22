using System;
using System.Threading;

public class Stylist
{
    private SemaphoreSlim StylistReady;
    private SemaphoreSlim GetsStyled;
    private SemaphoreSlim StylistDone;
    public string Name { get; set; }

    public Stylist(string name, SemaphoreSlim stylistReady, SemaphoreSlim getsStyled, SemaphoreSlim stylistDone)
    {
        Name = name;
        this.StylistReady=stylistReady;
        this.GetsStyled =getsStyled;
        this.StylistDone=stylistDone;
    }

    public void Run()
    {
        while (true)
        {
            StylistReady.Release();
            GetsStyled.Wait();
            StyleCustomer();
            StylistDone.Release();
        }
    }

    private void StyleCustomer()
    {
        Console.WriteLine($"{Name}: styling customer...");
        Thread.Sleep(3000);
        Console.WriteLine($"{Name}: styling done");
    }
}



public class Photographer
{
    private SemaphoreSlim PhotographerReady;
    private SemaphoreSlim TakesPhotos;
    private SemaphoreSlim PhotographerDone;
    public string Name { get; set; }

    public Photographer(string name, SemaphoreSlim photographerReady, SemaphoreSlim takesPhotos, SemaphoreSlim photographerDone)
    {
        Name = name;
        this.PhotographerReady=photographerReady;
        this.TakesPhotos=takesPhotos;
        this.PhotographerDone=photographerDone;
    }

    public void Run()
    {
        while (true)
        {
            PhotographerReady.Release();
            TakesPhotos.Wait();
            TakePhotos();
            PhotographerDone.Release();
        }
    }

    private void TakePhotos()
    {
        Console.WriteLine($"{Name}: taking photos...");
        Thread.Sleep(5000);
        Console.WriteLine($"{Name}: photos done");
    }
}

public class Customer
{
    private SemaphoreSlim StylistReady;
    private SemaphoreSlim GetsStyled;
    private SemaphoreSlim StylistDone;
    private SemaphoreSlim PhotographerReady;
    private SemaphoreSlim TakesPhotos;
    private SemaphoreSlim PhotographerDone;
    public string Name { get; set; }

    private Stylist stylist;

    public Customer(string name, SemaphoreSlim stylistReady, SemaphoreSlim getsStyled, SemaphoreSlim stylistDone, SemaphoreSlim photographerReady, SemaphoreSlim takesPhotos, SemaphoreSlim photographerDone)
    {
        Name = name;
        this.StylistReady=stylistReady;
        this.GetsStyled =getsStyled;
        this.StylistDone=stylistDone;
        this.PhotographerReady=photographerReady;
        this.TakesPhotos=takesPhotos;
        this.PhotographerDone=photographerDone;
    }

    public void Run()
    {
        Console.WriteLine($"{Name}: enters studio");
        StylistReady.Wait();
        Console.WriteLine($"{Name}: goes to stylist");
        GetsStyled.Release();
        StylistDone.Wait();
        Console.WriteLine($"{Name}: waits for photoshoot");
        PhotographerReady.Wait();
        Console.WriteLine($"{Name}: goes to photographer");
        TakesPhotos.Release();
        PhotographerDone.Wait();
        Console.WriteLine($"{Name}: leaves studio");
    }
}

class Program
{
    static void Main(string[] args)
    {
        SemaphoreSlim StylistReady=new SemaphoreSlim(0);
        SemaphoreSlim GetsStyled=new SemaphoreSlim(0);
        SemaphoreSlim StylistDone=new SemaphoreSlim(0);
        SemaphoreSlim PhotographerReady=new SemaphoreSlim(0);
        SemaphoreSlim TakesPhotos=new SemaphoreSlim(0);
        SemaphoreSlim PhotographerDone=new SemaphoreSlim(0);
        Stylist s = new Stylist("John", StylistReady,  GetsStyled, StylistDone);
        Photographer[] p =  new Photographer[]
        {
            new Photographer("Max", PhotographerReady, TakesPhotos, PhotographerDone),
            new Photographer("Moritz", PhotographerReady, TakesPhotos, PhotographerDone),
        };
        Customer c = new Customer("Jonas",StylistReady, GetsStyled, StylistDone, PhotographerReady, TakesPhotos, PhotographerDone);
        new Thread(s.Run).Start();
        foreach (Photographer ph in p)
        {
            new Thread(ph.Run).Start();
        }
        new Thread(c.Run).Start();
    }
}