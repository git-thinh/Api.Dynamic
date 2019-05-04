using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NamedPipeClient
{
    class Program
    {
        static void Main(string[] args)
        { 
            //Client
            var client = new NamedPipeClientStream("PipesOfPiece");
            client.Connect();
            //StreamReader reader = new StreamReader(client);
            StreamWriter writer = new StreamWriter(client);

            while (true)
            {
                string input = Console.ReadLine();
                if (String.IsNullOrEmpty(input)) break;
                writer.WriteLine(input);
                writer.Flush();
                //Console.WriteLine(reader.ReadLine());
            }



        }
    }
}
