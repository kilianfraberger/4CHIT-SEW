using System.Net;
using System.Net.Sockets;

TcpListener lsnr = new TcpListener(IPAddress.Loopback, 2025);
lsnr.Start();
const int ANZ = 5;

Console.WriteLine("Server started, listening on port 2025");
for (int i = 0; i < ANZ; i++)
{
    new Thread(() => HTTPFileServer()).Start();
}


void HTTPFileServer()
{
    try{
    Socket soc = lsnr.AcceptSocket();
    Console.WriteLine($"Connected: {soc.RemoteEndPoint}");
    
    Stream s = new NetworkStream(soc);
    StreamReader sr = new StreamReader(s);
    StreamWriter sw = new StreamWriter(s);
    sw.AutoFlush = true;

    string request = sr.ReadLine();
    //rquest muss HTTP conform sein: GET /a.txt HTTP/1.1
    
    Console.WriteLine($"Request: {request}");
    var wosawü = request.Split(' ');
    string file = wosawü[1].Substring(1);
    
    string content = File.ReadAllText(file);
    //content in HTTP Header verpacken
    string header = "HTTP/1.1 200 OK\n";
    header += "Content-type: text/plain; charset=utf-8\n";
    header += $"content-length: {content.Length}\n";
    Console.WriteLine($"Sending: {header + "\n" + content}");
    sw.WriteLine(header + "\n" + content);
    }
    catch(Exception e){}
}


void SessionKeepingFileServer()     //wenn man a.txt bzw. b.txt requested kommt der Inhalt der Datei zurück 
{
    while (true)
    {
        Socket soc = lsnr.AcceptSocket();
        try
        {
            Console.WriteLine($"Connected: {soc.RemoteEndPoint}");

            Stream s = new NetworkStream(soc);
            StreamReader sr = new StreamReader(s);
            StreamWriter sw = new StreamWriter(s);
            sw.AutoFlush = true;

            while (true)
            {
                string input = sr.ReadLine();
                Console.WriteLine($"Client requested: {input}");
            
                string content = File.ReadAllText(input);
                Console.WriteLine($"Sent: {content}");
                
                sw.WriteLine(content);
                sw.Flush();
            }
        }
        
        catch (Exception e)
        {
            Console.WriteLine($"Broke: {e.Message}");
        }
        finally
        {
            soc.Close();
        }
    }
}

void SessionlessFileServer()     //wenn man a.txt bzw. b.txt requested kommt der Inhalt der Datei zurück 
{
    while (true)
    {
        Socket soc = lsnr.AcceptSocket();
        try
        {
            Console.WriteLine($"Connected: {soc.RemoteEndPoint}");

            Stream s = new NetworkStream(soc);
            StreamReader sr = new StreamReader(s);
            StreamWriter sw = new StreamWriter(s);
            sw.AutoFlush = true;

            string input = sr.ReadLine();
            Console.WriteLine($"Client requested: {input}");
            
            string content = File.ReadAllText(input);
            
            sw.WriteLine(content);
            sw.Flush();

        }
        catch (Exception e)
        {
            Console.WriteLine($"Broke: {e.Message}");
        }
        finally
        {
            soc.Close();
        }
    }
}

void StatelessEchoClient()  //Echo Client
{
    while (true)
    {
        Socket soc = lsnr.AcceptSocket();
        try
        {
            Console.WriteLine($"Connected: {soc.RemoteEndPoint}");

            Stream s = new NetworkStream(soc);
            StreamReader sr = new StreamReader(s);
            StreamWriter sw = new StreamWriter(s);
            sw.AutoFlush = true;

            string input = sr.ReadLine();
            Console.WriteLine($"Client requested: {input}");
            sw.WriteLine(input.ToUpper());
            sw.Flush();

        }
        catch (Exception e)
        {
            Console.WriteLine($"Broke: {e.Message}");
        }
        finally
        {
            soc.Close();
        }
    }
}
 