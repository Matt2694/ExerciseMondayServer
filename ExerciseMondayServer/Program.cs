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
            Program p = new Program();
            p.Run();
        }

        private void Run()
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

        private static void HandleClient(Client TCPClient)
        {
            StreamReader sr = new StreamReader(TCPClient.client.GetStream());
            StreamWriter sw = new StreamWriter(TCPClient.client.GetStream());
            sw.AutoFlush = true;
            sw.WriteLine("Guess a number between 1 and 10.");
            int count;
            bool outer = true;
            bool inner;

            while (outer)
            {
                count = 0;
                inner = true;
                Random random = new Random();
                int randomNumber = random.Next(1, 11);
                while (inner)
                {
                    count++;
                    try
                    {
                        string stringRequest = sr.ReadLine();
                        bool parse = int.TryParse(stringRequest, out int number);
                        if (parse)
                        {
                            if (number == randomNumber)
                            {
                                sw.WriteLine("Great, just {0} guess(es)", count);
                                inner = false;
                            }
                            else if (number != randomNumber && count < 10)
                            {
                                sw.WriteLine("Wrong answer, {0} tries left", 10 - count);
                            }
                            else
                            {
                                sw.WriteLine("You didn't manage to guess the right number");
                                inner = false;
                            }
                        }
                        else
                        {
                            if (stringRequest == "exit")
                            {
                                TCPClient.client.Close();
                                Thread.CurrentThread.Abort();
                            }
                            else
                            {
                                sw.WriteLine("Invalid command, exiting program.");
                                throw new Exception();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is ThreadAbortException))
                        {
                            Console.WriteLine(" >> " + ex.ToString());
                            TCPClient.client.Close();
                            Thread.CurrentThread.Abort();
                        }
                    }
                }
                sw.WriteLine("Would you like to playy again? y or n");
                string response = sr.ReadLine();
                if (response.Equals("n"))
                {
                    outer = false;
                }
            }
            TCPClient.client.Close();
            Thread.CurrentThread.Abort();
        }

        //public static int Addition(string num1, string num2)
        //{
        //    return int.Parse(num1) + int.Parse(num2);
        //}

        //public static int Subtract(string num1, string num2)
        //{
        //    return int.Parse(num1) - int.Parse(num2);
        //}

        
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

