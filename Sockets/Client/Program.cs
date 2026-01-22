using System.Net.Sockets;

TcpClient client = new TcpClient("localhost", 2025);

Stream stream = client.GetStream();

Console.WriteLine($"Connected to server on {client.Client.LocalEndPoint}");

StreamReader reader = new StreamReader(stream);
StreamWriter writer = new StreamWriter(stream);

writer.AutoFlush = true;

while (true)
{
    writer.WriteLine(Console.ReadLine());
    string response = reader.ReadLine();
    Console.WriteLine($"Response: {response}");
}


Console.ReadKey();