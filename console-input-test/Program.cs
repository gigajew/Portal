using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace console_input_test
{
    public class ConsoleHelper
    {

        private int lastWriteCursorTop = 0;
        private Queue<string> messagesToBePosted = new Queue<string>();
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

                }
            }).Start();
        }

        public void WriteLine(string message, params string[] parameters)
        {
            lock (messagesToBePosted)
                messagesToBePosted.Enqueue(string.Format(message, parameters));
        }

        private void Log(string message, params string[] parameters)
        {
            string formattedMessage = string.Format(message, parameters);

            int messageLines = formattedMessage.Length / Console.BufferWidth + 1;
            int inputBufferLines = Console.CursorTop - lastWriteCursorTop + 1;

            Console.MoveBufferArea(sourceLeft: 0, sourceTop: lastWriteCursorTop,
                                   targetLeft: 0, targetTop: lastWriteCursorTop + messageLines,
                                   sourceWidth: Console.BufferWidth, sourceHeight: inputBufferLines);

            int cursorLeft = Console.CursorLeft;
            Console.CursorLeft = 0;
            Console.CursorTop -= inputBufferLines - 1;
            Console.WriteLine(formattedMessage);
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

    class Program
    {
        static ConsoleHelper helper = new ConsoleHelper();
        static void Main(string[] args)
        {
            helper.Write += Helper_Write;
            helper.StartProcessingOutput();
            helper.StartAcceptingInput();
        }

        private static void Helper_Write(string obj)
        {
            throw new NotImplementedException();
        }
    }
}
