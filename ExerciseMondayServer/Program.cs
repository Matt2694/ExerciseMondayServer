using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Security.Permissions;

namespace ExerciseMondayServer
{
    class Program
    {
        private static List<Client> clients = new List<Client>();
        private static TcpListener listener = null;
        private static StreamReader reader = null;
        private static StreamWriter writer = null;
        private static List<Task> clientTasks = new List<Task>();
        private static List<string> messages = new List<string>();
        static void Main(string[] args)
        {
            try
            {
                listener = new TcpListener(50000);
                listener.Start();
                Console.WriteLine("Ready");
                //TcpClient client = listener.AcceptTcpClient();
                var connectTask = Task.Run(() => ConnectClients());
                Task.WaitAll(connectTask);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (listener != null)
                {
                    listener.Stop();
                }
            }
        }

        private static void ConnectClients()
        {
            Console.WriteLine("Waiting for incoming client connections...");
            while (true)
            {
                if (listener.Pending()) //if someone want to connect
                {
                    clients.Add(new Client(listener.AcceptTcpClient(), "Client: " + (clients.Count + 1)));
                    Console.WriteLine(clients[clients.Count - 1].clientName + " connected to server.");
                    clientTasks.Add(Task.Run(() => HandleClient(clients[clients.Count - 1]))); //start new task for new client
                }
            }
        }
        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        private static void HandleClient(Client TCPClient)
        {
            StreamReader sr = new StreamReader(TCPClient.client.GetStream());
            StreamWriter sw = new StreamWriter(TCPClient.client.GetStream());
            sw.AutoFlush = true;

            while (true)
            {
                try
                {
                    string stringRequest = sr.ReadLine();
                    string[] strArr = stringRequest.Split(' ');
                    if (strArr[0] == "add")
                    {
                        sw.WriteLine("sum " + Addition(strArr[1], strArr[2]));
                        sw.Flush();
                    }
                    else if (strArr[0] == "sub")
                    {
                        sw.WriteLine("difference " + Subtract(strArr[1], strArr[2]));
                        sw.Flush();
                    }
                    else if (strArr[0] == "exit")
                    {
                        Console.WriteLine("Connection Closed");
                        TCPClient.client.Close();
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }

            }
        }

        public static int Addition(string num1, string num2)
        {
            return int.Parse(num1) + int.Parse(num2);
        }

        public static int Subtract(string num1, string num2)
        {
            return int.Parse(num1) - int.Parse(num2);
        }

        
    }
    class Client
    {
        public TcpClient client;
        public StreamWriter writer; //write to client
        public StreamReader reader;
        public string clientName;

        public Client(TcpClient client, string clientName)
        {
            this.client = client;
            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());
            this.clientName = clientName;
        }
    }
}

