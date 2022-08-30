using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace portal_server
{
    internal class Program
    {
        public static ConsoleHelper con = new ConsoleHelper();
        static List<Client> clients = new List<Client>();
        static Socket serverSocket;
        const int serverPort = 25;
        static bool shouldRun = true;

        static void Main(string[] args)
        {
            con.StartProcessingOutput();
            con.StartAcceptingInput();
            con.Write += HandleUserInput;
            PostHelloMessage();

            ThreadPool.QueueUserWorkItem((a) =>
            {
                Init();
            });
        }

        private static void PostHelloMessage()
        {
            Console.Title = "portal server running on port "  + serverPort.ToString();
            con.WriteLineColor("welcome to portal.", ConsoleColor.Red);
            con.WriteLineColor("your available commands are as follows:", ConsoleColor.Yellow);
            con.WriteLineColor("\tmessagebox <your message here>\t\tsends a message to all connected users", ConsoleColor.Yellow);
            con.WriteLineColor("\topen\t\t\t\t\topens all clients cd-trays", ConsoleColor.Yellow);
            con.WriteLineColor("\tclose\t\t\t\t\tcloses all clients cd-trays", ConsoleColor.Yellow);
            con.WriteLineColor("\tlist clients\t\t\t\tlists all connected clients", ConsoleColor.Yellow);
        }

        private static void HandleUserInput(string whatever)
        {
            // this needs a lot of work to be prettier

            con.WriteLine(whatever);
            if (whatever.StartsWith("messagebox"))
            {
                con.WriteLine("messaging all users: {0}", whatever);
                lock( clients)
                {
                    foreach(var client in clients)
                    {
                        var messageFirstSpace = whatever.IndexOf(' ');
                        var messageArguments = whatever.Substring(messageFirstSpace + 1, whatever.Length - messageFirstSpace  - 1);
                        client.Send("200 {0}", messageArguments );
                    }
                }
            }else if (whatever.Trim().StartsWith("list clients"))
            {

                lock (clients )
                {
                    con.WriteLineColor("there are currently {0} clients connected", ConsoleColor.Yellow, clients.Count.ToString());
                    foreach (var client in clients)
                    {
                        con.WriteLineColor("\tclient: {0}", ConsoleColor.Yellow, client.clientSocket.RemoteEndPoint.ToString());
                    }
                }
               
            }else if (whatever.Trim().StartsWith("open"))
            {
                con.WriteLine("opening all the trays!");
                lock (clients)
                {
                    foreach (var client in clients)
                    {
                        client.Send("201 nothing");
                    }
                }
            }
            else if (whatever.Trim().StartsWith("close"))
            {
                con.WriteLine("closing all the trays!");
                lock (clients)
                {
                    foreach (var client in clients)
                    {
                        client.Send("202 nothing");
                    }
                }
            }
        }

        static void Init()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, serverPort));
            serverSocket.Listen(100);

            do
            {
                try
                {
                    Socket connected = serverSocket.Accept();
                    if (connected != null && connected.Connected)
                    {
                        Client client = new Client(connected);
                        client.Disconnect += HandleClientDisconnect;
                        clients.Add(client);
                        ThreadPool.QueueUserWorkItem((a) => {
                            client.Start();
                        });

                        con.WriteLine("client connected");
                    }
    
                    // spawn a new thread for receiving data per socket
                }
                catch (Exception e)
                {
                    con.WriteLine(e.Message);
                }
            } while (shouldRun);

        }

        static void HandleClientDisconnect(Client client, string message)
        {
            con.WriteLine(message);
            lock(clients)
            {
                clients.Remove(client);
            }
            client.Disconnect -= HandleClientDisconnect;
            client = null;
        }

    }
    class Client
    {
        static Encoding internetEncoding = new UTF8Encoding(false, false);
        public Socket clientSocket;
        NetworkStream clientBaseStream;
        StreamReader clientStreamReader;
        StreamWriter clientStreamWriter;
        bool clientShouldRun = true;
        public event Action<Client,string> Disconnect;

        public Client(Socket s)
        {
            this.clientSocket = s;
        }

        public void Start()
        {
            clientBaseStream = new NetworkStream(clientSocket);
            clientStreamReader = new StreamReader(clientBaseStream, internetEncoding);
            clientStreamWriter = new StreamWriter(clientBaseStream, internetEncoding);

            do
            {
                ReceiveCommands();
            } while (clientShouldRun && clientSocket.Connected);
        }

         void ProcessCommand(Command command)
        {
            switch (command.code)
            {
                case 200:
                    Program.con.WriteLine("client said OK");
                    break;
            }
        }
        
        public void Send(string message, params string[] arguments)
        {
            clientStreamWriter.WriteLine(message, arguments);
            clientStreamWriter.Flush();
            Program.con.WriteLine("client has successfully sent message: {0}", string.Format(message, arguments));
        }

         void ReceiveCommands()
        {
            try
            {
                string line = clientStreamReader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    Match match = Regex.Match(line, "^(\\d+)\\s(.*?)$");
                    if (match.Success)
                    {
                        Command cmd = new Command()
                        {
                            code = int.Parse(match.Groups[1].Value),
                            message = match.Groups[2].Value
                        };
                        ProcessCommand(cmd);
                    }
                }
            }
            catch (Exception e)
            {
                Program.con.WriteLine(e.Message);

                if(!clientSocket.Connected )
                {
                    clientShouldRun = false;
                    Disconnect?.Invoke(this, string.Format( "a client error has occured: {0}", e.Message));
                }
            }
        }

    }

    class Command
    {
        public int code;
        public string message;
    }
}
