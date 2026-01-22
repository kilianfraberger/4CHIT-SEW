using System.Net;
using System.Net.Sockets;

TcpListener lsnr = new TcpListener(IPAddress.Loopback, 2025);
lsnr.Start();
const int ANZ = 5;

Console.WriteLine("Server started, listening on port 2025");
for (int i = 0; i < ANZ; i++)
{
    new Thread(() => HTTPImageServer()).Start();
}


void HTTPImageServer()
{
    try
    {
        using Socket soc = lsnr.AcceptSocket();
        Console.WriteLine($"Connected: {soc.RemoteEndPoint}");

        using Stream s = new NetworkStream(soc);
        // Wir brauchen den StreamReader nur noch, um den Request (Text) zu lesen
        StreamReader sr = new StreamReader(s);
        
        string request = sr.ReadLine();
        if (string.IsNullOrEmpty(request)) return;

        Console.WriteLine($"Request: {request}");
        var wosawü = request.Split(' ');
        string fileName = wosawü[1].Substring(1);

        if (File.Exists(fileName))
        {
            // 1. Datei binär einlesen
            byte[] fileBytes = File.ReadAllBytes(fileName);
            
            // 2. Mime-Type bestimmen (sehr simpel gelöst)
            string contentType = fileName.EndsWith(".jpg") ? "image/jpeg" : 
                fileName.EndsWith(".png") ? "image/png" : "text/plain";

            // 3. Header als Text vorbereiten (Header müssen immer mit \r\n enden)
            string header = "HTTP/1.1 200 OK\r\n" +
                            $"Content-Type: {contentType}\r\n" +
                            $"Content-Length: {fileBytes.Length}\r\n" +
                            "Connection: close\r\n" +
                            "\r\n"; // Leerzeile trennt Header von Body

            // 4. Header senden (als Bytes)
            byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
            s.Write(headerBytes, 0, headerBytes.Length);

            // 5. Bilddaten (Body) senden
            s.Write(fileBytes, 0, fileBytes.Length);
        }
        else
        {
            // 404 Fehler senden, falls Datei nicht existiert
            byte[] error = System.Text.Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\nFile not found.");
            s.Write(error, 0, error.Length);
        }
    }
    catch (Exception e) 
    {
        Console.WriteLine($"Error: {e.Message}");
    }
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
 