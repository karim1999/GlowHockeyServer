using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace GlowHokeyServer
{
    internal class Program
    {
        public static List<TcpClient> clients= new List<TcpClient>();
        public static void Main(string[] args)
        {
            TcpListener listener= new TcpListener(6691);
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine(client.Client.RemoteEndPoint);
                
                StreamReader sr= new StreamReader(client.GetStream());
                StreamWriter sw= new StreamWriter(client.GetStream());

                while (true)
                {
                    String clientMsg = sr.ReadLine();
                    if (clientMsg == "join")
                    {
                        clients.Add(client);
                    }
                    Console.WriteLine("Sent from client: "+ clientMsg);

                    String msg = "sdfsd";
                    sw.WriteLine(msg);
                    
                    sw.Flush();
                }
                sr.Close();
                sw.Close();
                client.Close();
            }
        }
    }
}