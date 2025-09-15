using System;
using System.Threading;

using System;
using System.Threading;

using System;
using System.Net.NetworkInformation;
using System.Threading;

class Program
{
    public static bool finished = false;
    public static bool thread1Waiting = false;
    public static bool thread2Waiting = false;

    static void Main(string[] args)
    {
        PingPongBall ball = new PingPongBall();

        PlayerPing ping = new PlayerPing(ball);
        PlayerPong pong = new PlayerPong(ball);

        Thread thread1 = new Thread(new ThreadStart(ping.MakePing));
        Thread thread2 = new Thread(new ThreadStart(pong.MakePong));

        thread1.Start();
        thread2.Start();

        Console.ReadLine();
        finished = true;
    }
}

class PingPongBall
{
    public string Text { get; set; }
}

class PlayerPing
{
    private PingPongBall obj;

    public PlayerPing(PingPongBall obj)
    {
        this.obj = obj;
    }

    public void MakePing()
    {
        Monitor.Enter(obj);
        for (int i = 0; i < 10; i++)
        {
            Program.thread1Waiting = true;
            
            if (Program.thread2Waiting == false)
                Monitor.Wait(obj);

            obj.Text = "Ping";
            Console.WriteLine(obj.Text);
            
            Monitor.Pulse(obj);
            Program.thread2Waiting = false;
        }
        Program.finished = true;
        Monitor.Exit(obj);
    }
}

class PlayerPong
{
    private PingPongBall obj;

    public PlayerPong(PingPongBall obj)
    {
        this.obj = obj;
    }

    public void MakePong()
    {
        Monitor.Enter(obj);
        
        if (Program.thread1Waiting)
            Monitor.Pulse(obj);
        
        Program.thread2Waiting = true;

        while (Monitor.Wait(obj))
        {
            Console.WriteLine("Pong");
            
            Monitor.Pulse(obj);

            if (Program.finished)
                Thread.CurrentThread.Abort();
        }

        Monitor.Exit(obj);
    }
}
