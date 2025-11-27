using System;
using System.Threading;

public class Car
{
    public string Racer { get; set; }
    private SemaphoreSlim startSignal;
    private SemaphoreSlim pitStop;
    private SemaphoreSlim readyCounter;
    private SemaphoreSlim finishSemaphore;
    private static Random rand = new Random();

    public Car(
        string racer, 
        SemaphoreSlim startSignal, 
        SemaphoreSlim pitStop, 
        SemaphoreSlim readyCounter, 
        SemaphoreSlim finishSemaphore)
    {
        Racer = racer;
        this.startSignal = startSignal;
        this.pitStop = pitStop;
        this.readyCounter = readyCounter;
        this.finishSemaphore = finishSemaphore;
    }

    public void Run()
    {
        WaitForSignal();
        readyCounter.Release();
        startSignal.Wait();
        Race();
        pitStop.Wait(); 
        TakingPitstop();
        pitStop.Release();
        Race();

        finishSemaphore.Release();
    }

    private void WaitForSignal()
    {
        Console.WriteLine($"{Racer}: Waiting for Start Signal");
        Thread.Sleep(rand.Next(150, 301));  
    }

    private void Race()
    {
        Console.WriteLine($"{Racer}: Racing");
        Thread.Sleep(rand.Next(1200, 2001)); 
    }

    private void TakingPitstop()
    {
        Console.WriteLine($"{Racer}: Taking Pitstop");
        Thread.Sleep(rand.Next(300, 701)); 
    }
}

public class F1Race
{
    private const int TotalCars = 5;
    private SemaphoreSlim readyCounter = new SemaphoreSlim(0);
    private SemaphoreSlim startSignal = new SemaphoreSlim(0);
    private SemaphoreSlim pitStop = new SemaphoreSlim(3);
    private SemaphoreSlim finishSemaphore = new SemaphoreSlim(0);
    private static Random rand = new Random();

    public void Run()
    {
        Car[] cars = new Car[TotalCars];

        

        for (int i = 0; i < TotalCars; i++)
        {
            cars[i] = new Car(
                $"Car{i+1}", 
                startSignal, 
                pitStop, 
                readyCounter, 
                finishSemaphore);
            
            new Thread(cars[i].Run).Start();
        }
        
        for (int i = 0; i < TotalCars; i++)
            readyCounter.Wait();

        Start();
        
        startSignal.Release(TotalCars);
        
        for (int i = 0; i < TotalCars; i++)
            finishSemaphore.Wait();

        End();
    }

    private void Start()
    {
        Thread.Sleep(rand.Next(800, 1500));
        Console.WriteLine("Start Race");
    }

    private void End()
    {
        Console.WriteLine("Race finished");
    }
}

class Program
{
    static void Main(string[] args)
    {
        F1Race race = new F1Race();
        race.Run();
    }
}
