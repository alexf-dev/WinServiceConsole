using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinServiceConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Factory.StartNew(() =>
            {                
                while (true)
                {
                    var server = new NamedPipeServerStream("WinServiceConsole");
                    server.WaitForConnection();
                    StreamReader reader = new StreamReader(server);
                    StreamWriter writer = new StreamWriter(server);
                    var line = reader.ReadLine();
                    Console.WriteLine(line);
                    writer.Flush();
                    server.Disconnect();
                    server.Close();
                }
            });

            Console.ReadLine();
        }
    }
}
