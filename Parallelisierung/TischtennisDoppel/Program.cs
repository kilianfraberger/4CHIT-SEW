// See https://aka.ms/new-console-template for more information

public class Player
{
    public string Name { get; set; }
    public SemaphoreSlim PlayerPlay;
    Random random = new Random();

    public Player(SemaphoreSlim PlayerPlay, string Name)
    {
        this.PlayerPlay = PlayerPlay;
        this.Name = Name;
    }

    public void Run()
    {
        while (true)
        {
            PlayerPlay.Wait();
            Console.WriteLine($"Player {Name} is playing");
            Thread.Sleep(random.Next(3000));
        }
    }
}

public class TeamA
{
    public SemaphoreSlim PlayerPlay=new SemaphoreSlim(0);
    private Player[] players=new Player[2];
    private TeamB b;
    private SemaphoreSlim BallA;
    private SemaphoreSlim BallB;

    public TeamA(SemaphoreSlim BallA, SemaphoreSlim BallB)
    {
        this.BallA = BallA;
        this.BallB = BallB;
        players[0]=new Player(PlayerPlay, "Kilian");
        players[1]=new Player(PlayerPlay, "Manuel");
        new Thread(players[0].Run).Start();
        new Thread(players[1].Run).Start();
    }

    public void Run()
    {
        while (true)
        {
            BallA.Wait();
            PlayerPlay.Release();
            Thread.Sleep(1000);
            Console.WriteLine("Team A played");
            BallB.Release();
        }
    }
}

public class TeamB
{
    public SemaphoreSlim PlayerPlay=new SemaphoreSlim(0);
    private Player[] players=new Player[2];
    private TeamA a;
    private SemaphoreSlim BallB;3
    private SemaphoreSlim BallA;

    public TeamB(SemaphoreSlim BallB, SemaphoreSlim BallA)
    {
        this.BallB=BallB;
        this.BallA=BallA;
        players[0]=new Player(PlayerPlay, "Kilian");
        players[1]=new Player(PlayerPlay, "Manuel");
        new Thread(players[0].Run).Start();
        new Thread(players[1].Run).Start();
    }

    public void Run()
    {
        while (true)
        {
            BallB.Wait();
            PlayerPlay.Release();
            Thread.Sleep(1000);
            Console.WriteLine("Team B played");
            BallA.Release();
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
    SemaphoreSlim BallA=new SemaphoreSlim(1);
    SemaphoreSlim BallB=new SemaphoreSlim(0);
        TeamA a = new TeamA(BallA, BallB);
        TeamB b = new TeamB(BallB, BallA);
        new Thread(a.Run).Start();
        new Thread(b.Run).Start();
    }}