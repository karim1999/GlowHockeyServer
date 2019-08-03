﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

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
        bool isConnected= true;
        TcpClient opponent;
        public ClientHandler(TcpClient soc)
        {
            this.soc = soc;
            clients.Enqueue(soc);
        }


        public void handle()
        {
            StreamReader sr = new StreamReader(soc.GetStream());
            StreamWriter sw = new StreamWriter(soc.GetStream());

            while (isConnected)
            {
                foreach (Pair pair in pairs)
                {
                    if (pair.client1 == soc)
                    {
                        opponent = pair.client2;
                        pair.isClient1In = true;
                        isConnected = false;
                    }
                    else if (pair.client2 == soc)
                    {
                        opponent = pair.client1;
                        pair.isClient2In = true;
                        isConnected = false;
                    }

                    if (!isConnected)
                    {
                        sw.WriteLine(opponent.Client.RemoteEndPoint.ToString());
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
    }


    class Program
    {
        static void Main(string[] args)
        {
            //Create Tcp Listener
            //waiting for clients
            TcpListener server = TcpListener.Create(6691);
            server.Start();
            Console.WriteLine("The Server is running ......");

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
}