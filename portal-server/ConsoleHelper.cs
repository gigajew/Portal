using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
/*
 * 
 * All credits to Tera
 * https://stackoverflow.com/questions/849876/c-sharp-simultanous-console-input-and-output
 * 
 */

namespace portal_server
{
    public class ConsoleHelper
    {

        private int lastWriteCursorTop = 0;
        private Queue<ConsoleMessage> messagesToBePosted = new Queue<ConsoleMessage>();
        public event Action<string> Write;


        public void StartAcceptingInput()
        {
            // Reader
            new Thread(() =>
            {
                string line;
                while ((line = Read()) != null)
                {
                    if (Write != null)
                        Write(line);
                }
                Environment.Exit(0);
            }).Start();
        }

        public void StartProcessingOutput()
        {
            // Writer
            new Thread(() =>
            {
                while (true)
                {
                    //Thread.Sleep(1000);
                    //Log("we're taking the hobbits to Isengard!");
                    lock (messagesToBePosted)
                    {
                        if (messagesToBePosted.Count > 0)
                        {
                            Log(messagesToBePosted.Dequeue());
                        }
                    }
                    Thread.Sleep(10);
                }
            }).Start();
        }

        public void WriteLineColor(string message, ConsoleColor color, params string[] parameters)
        {
            ConsoleMessage special = ConsoleMessage.Create(message, parameters);
            special.color = color;
            lock (messagesToBePosted)
                messagesToBePosted.Enqueue(special);
        }

        public void WriteLine(string message, params string[] parameters)
        {
            lock (messagesToBePosted)
                messagesToBePosted.Enqueue(ConsoleMessage.Create(message, parameters));
        }

        private void Log(ConsoleMessage message)
        {
            int messageLines = message.message.Length / Console.BufferWidth + 1;
            int inputBufferLines = Console.CursorTop - lastWriteCursorTop + 1;

            Console.MoveBufferArea(sourceLeft: 0, sourceTop: lastWriteCursorTop,
                                   targetLeft: 0, targetTop: lastWriteCursorTop + messageLines,
                                   sourceWidth: Console.BufferWidth, sourceHeight: inputBufferLines);

            int cursorLeft = Console.CursorLeft;
            Console.CursorLeft = 0;
            Console.CursorTop -= inputBufferLines - 1;

            // save old color
            ConsoleColor oldForeground = Console.ForegroundColor;

            // change color
            Console.ForegroundColor = message.color;

            // write message
            Console.WriteLine(message.message);

            // restore old color
            Console.ForegroundColor = oldForeground;

            lastWriteCursorTop = Console.CursorTop;
            Console.CursorLeft = cursorLeft;
            Console.CursorTop += inputBufferLines - 1;
        }
        private string Read()
        {
            Console.Write(">"); // optional
            string line = Console.ReadLine();
            lastWriteCursorTop = Console.CursorTop;
            return line;
        }
    }

    public class ConsoleMessage
    {
        public string message;
        public ConsoleColor color = Console.ForegroundColor;

        public static ConsoleMessage Create(string msg, params string[] pars)
        {
            ConsoleMessage instance = new ConsoleMessage();
            instance.message = string.Format(msg, pars);
            return instance;
        }
    }
}
