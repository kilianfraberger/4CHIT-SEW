class Program
{
    static object locker = new object();
    static bool pingTurn = true; // wer ist dran? true = Ping, false = Pong
    static int rounds = 10;       // wie oft soll Ping/Pong ausgegeben werden?

    static void Main()
    {
        Thread t1 = new Thread(Spieler1);
        Thread t2 = new Thread(Spieler2);

        t1.Start();
        t2.Start();

        t1.Join();
        t2.Join();

        Console.WriteLine("Fertig!");
    }

    static void Spieler1()
    {
        for (int i = 0; i < rounds; i++)
        {
            lock (locker)
            {
                while (!pingTurn) Monitor.Wait(locker); // warten, bis Ping dran ist
                Console.WriteLine("Ping");
                pingTurn = false; // jetzt ist Pong dran
                Monitor.Pulse(locker); // wecke Pong
            }
        }
    }

    static void Spieler2()
    {
        for (int i = 0; i < rounds; i++)
        {
            lock (locker)
            {
                while (pingTurn) Monitor.Wait(locker); // warten, bis Pong dran ist
                Console.WriteLine("Pong");
                pingTurn = true; // jetzt ist Ping dran
                Monitor.Pulse(locker); // wecke Ping
            }
        }
    }
}