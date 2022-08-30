using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace portal
{
    internal class Program
    {
        static Encoding internetEncoding = new UTF8Encoding(false, false);
        static Socket clientSocket;
        static NetworkStream clientBaseStream;
        static StreamReader clientStreamReader;
        static StreamWriter clientStreamWriter;
        const string remoteHost = "localhost";
        const int remotePort = 25;
        static bool clientShouldRun = true;

        static void Main(string[] args)
        {
            AttemptConnect();
            if (clientSocket != null)
            {
                while (clientSocket.Connected)
                {
                    ReceiveCommands();
                }
            }

            // stay alive
            Console.Read();
        }


        static void ProcessCommand(Command command)
        {
            switch (command.code)
            {
                case 200:
                    MessageBox.Show(command.message);
                    break;
                case 201:
                    Console.WriteLine("opening tray");
                    CDTray.Open();
                    Send("200 OK");
                    break;
                case 202:
                    Console.WriteLine("closing tray");
                    CDTray.Close();
                    Send("200 OK");
                    break;
            }
        }

        static void Send(string strMessage)
        {
            clientStreamWriter.WriteLine(strMessage);
            clientStreamWriter.Flush();
        }

        static void ReceiveCommands()
        {
            try
            {
                string line = clientStreamReader.ReadLine(); // thread locker
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
                Log(e.Message);
            }
        }

        static void AttemptConnect()
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(remoteHost, remotePort);
                if (clientSocket.Connected)
                {
                    Log("successfully connected to {0}", remoteHost);

                    clientBaseStream = new NetworkStream(clientSocket);
                    clientStreamReader = new StreamReader(clientBaseStream, internetEncoding);
                    clientStreamWriter = new StreamWriter(clientBaseStream, internetEncoding);

                }

            }
            catch (Exception e)
            {
                Log(e.Message);
            }
        }
        static void Log(string message, params string[] arguments)
        {
            Console.WriteLine(message, arguments);
        }

        static void CloseConnection()
        {
            clientSocket.Close();
            clientSocket = null;
        }
    }

    class Command
    {
        public int code;
        public string message;
    }
}
