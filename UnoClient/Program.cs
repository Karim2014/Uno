using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UnoClient {
    class Program {

        static void Main(string[] args) {

            Console.Write("Введите свое имя: ");
            string name = Console.ReadLine();
            var client = new Client(name);

        }
    }
}
