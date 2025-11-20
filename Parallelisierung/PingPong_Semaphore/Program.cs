SemaphoreSlim ping = new SemaphoreSlim(1, 1);
//Ping Fahne ist oben
SemaphoreSlim pong = new SemaphoreSlim(0, 1);
//Pong Fahne ist unten
bool stop = false;
new Thread(new ThreadStart(Ping)).Start();
new Thread(new ThreadStart(Pong)).Start();


void Ping()
{
    for (int i = 0; i < 10; i++)
    {
        ping.Wait(); //eigne Fahne runter
        Console.WriteLine("ping");
        pong.Release(); //andere Fahne hinauf
    }
    stop = true;
}

void Pong()
{
    while (stop == false)
    {
        pong.Wait(); //eigene Fahne runter
        Console.WriteLine("pong");
        ping.Release(); //andere Fahne hinauf
    }
}