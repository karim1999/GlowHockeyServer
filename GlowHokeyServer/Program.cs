using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OpponentLibrary;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace GlowHokeyServer
{
    class Pair
    {
        public TcpClient client1;
        public TcpClient client2;
        public bool isClient1In = false;
        public bool isClient2In = false;
        public Pair(TcpClient client1, TcpClient client2)
        {
            this.client1 = client1;
            this.client2 = client2;
        }
    }
    class ClientHandler
    {
        public static ConcurrentQueue<TcpClient> clients = new ConcurrentQueue<TcpClient>();
        public static ConcurrentQueue<Pair> pairs = new ConcurrentQueue<Pair>();

        TcpClient soc;
        bool isConnected = true;
        TcpClient opponent;
        public ClientHandler(TcpClient soc)
        {
            this.soc = soc;
            clients.Enqueue(soc);
        }


        public void handle()
        {
            try
            {
                NetworkStream ns = soc.GetStream();
                StreamReader sr = new StreamReader(ns);
                StreamWriter sw = new StreamWriter(ns);
                BinaryFormatter bf = new BinaryFormatter();
                String type = "T";
                while (isConnected)
                {
                    if (!soc.Connected)
                    {
                        clients.TryDequeue(out soc);
                        Thread.Sleep(1);
                    }
                    foreach (Pair pair in pairs)
                    {
                        if (pair.client1 == soc)
                        {
                            opponent = pair.client2;
                            pair.isClient1In = true;
                            isConnected = false;
                            type = "T";
                        }
                        else if (pair.client2 == soc)
                        {
                            opponent = pair.client1;
                            pair.isClient2In = true;
                            isConnected = false;
                            type = "B";
                        }

                        if (!isConnected)
                        {
                            IPEndPoint ip = (IPEndPoint)opponent.Client.RemoteEndPoint;
                            IPEndPoint currentIp = (IPEndPoint)soc.Client.RemoteEndPoint;
                            //                        bf.Serialize(ns, new Opponent((IPEndPoint)opponent.Client.RemoteEndPoint, type, (IPEndPoint)soc.Client.RemoteEndPoint));
                            sw.WriteLine(ip.Address + "," + ip.Port + "," + type + "," + currentIp.Address + "," + currentIp.Port);
                            sw.Flush();
                            Console.WriteLine("======================");
                            break;
                        }

                    }

                }
                sr.Close();
                sw.Close();
                soc.Close();
            }
            catch(Exception e)
            {
                //clients.TryDequeue(out soc);
                Console.WriteLine("============================");
            }
        }
    }

    class TCPThread
    {
        int port;

        public TCPThread(int port)
        {
            this.port = port;
        }

        public void handle()
        {
            //Create Tcp Listener
            //waiting for clients
            TcpListener server = TcpListener.Create(port);
            server.Start();
            //Console.WriteLine("The Server is running ......");

            while (true)
            {
                //accept connection
                //blocking waiting for a connection
                TcpClient soc = server.AcceptTcpClient();
                Console.WriteLine(soc.Client.RemoteEndPoint);


                ClientHandler ch = new ClientHandler(soc);
                Thread t = new Thread(ch.handle);
                t.Start();

                if (ClientHandler.clients.Count >= 2)
                {
                    Console.WriteLine("Clients are more than 2");
                    TcpClient[] clientsArray = ClientHandler.clients.ToArray();
                    ClientHandler.pairs.Enqueue(new Pair(clientsArray[0], clientsArray[1]));
                    ClientHandler.clients.TryDequeue(out clientsArray[0]);
                    ClientHandler.clients.TryDequeue(out clientsArray[1]);
                }


            }

            server.Stop();

        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Application.Run(new Form1());
        }
    }
}