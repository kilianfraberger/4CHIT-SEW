using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener lsnr = new TcpListener(IPAddress.Loopback, 2025);
lsnr.Start();

Console.WriteLine("Server läuft auf http://localhost:2025");

while (true)
{
    Socket soc = lsnr.AcceptSocket();
    new Thread(() => HandleRequest(soc)).Start();
}

void HandleRequest(Socket soc)
{
    try
    {
        using (soc)
        using (Stream s = new NetworkStream(soc))
        {
            StreamReader sr = new StreamReader(s);
            string request = sr.ReadLine();
            if (string.IsNullOrEmpty(request)) return;

            Console.WriteLine($"Anfrage: {request}");

            var parts = request.Split(' ');
            if (parts.Length < 2) return;
            
            string fileName = parts[1].TrimStart('/'); 
            string extension = Path.GetExtension(fileName).ToLower();
            
            ResponseFactory factory = extension switch
            {
                ".jpg" or ".jpeg" => new BinaryResponseFactory("image/jpeg"),
                ".png"            => new BinaryResponseFactory("image/png"),
                ".gif"            => new BinaryResponseFactory("image/gif"),
                ".pdf"            => new BinaryResponseFactory("application/pdf"),
                ".html" or ".htm" => new TextResponseFactory("text/html"),
                ".csv"            => new TextResponseFactory("text/csv"),
                ".txt"            => new TextResponseFactory("text/plain"),
                _                 => new TextResponseFactory("text/plain") 
            };

            factory.SendResponse(s, fileName);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fehler: {ex.Message}");
    }
}

abstract class ResponseFactory
{
    public abstract string ContentType { get; }
    
    public void SendResponse(Stream s, string filePath)
    {
        if (!File.Exists(filePath))
        {
            SendError(s, 404, "Not Found", "Datei nicht gefunden.");
            return;
        }

        byte[] data = PrepareData(filePath);
        
        StringBuilder sb = new StringBuilder();
        sb.Append("HTTP/1.1 200 OK\r\n");
        sb.Append($"Content-Type: {ContentType}\r\n");
        sb.Append($"Content-Length: {data.Length}\r\n");
        sb.Append("Connection: close\r\n\r\n");

        byte[] headerBytes = Encoding.UTF8.GetBytes(sb.ToString());

        s.Write(headerBytes, 0, headerBytes.Length);
        s.Write(data, 0, data.Length);
        s.Flush();
    }

    protected abstract byte[] PrepareData(string filePath);

    private void SendError(Stream s, int code, string status, string msg)
    {
        byte[] body = Encoding.UTF8.GetBytes($"<h1>{code} {status}</h1><p>{msg}</p>");
        string head = $"HTTP/1.1 {code} {status}\r\nContent-Type: text/html\r\nContent-Length: {body.Length}\r\n\r\n";
        s.Write(Encoding.UTF8.GetBytes(head));
        s.Write(body);
    }
}

class BinaryResponseFactory : ResponseFactory
{
    private readonly string _mime;
    public BinaryResponseFactory(string mime) => _mime = mime;
    public override string ContentType => _mime;

    protected override byte[] PrepareData(string filePath) => File.ReadAllBytes(filePath);
}

class TextResponseFactory : ResponseFactory
{
    private readonly string _mime;
    public TextResponseFactory(string mime) => _mime = mime;
    public override string ContentType => _mime + "; charset=utf-8";

    protected override byte[] PrepareData(string filePath) => File.ReadAllBytes(filePath);
}