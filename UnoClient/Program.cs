using System;

namespace UnoClient {
    class Program {

        static void Main(string[] args) {

            Console.Write("Введите свое имя: ");
            string name = Console.ReadLine();
            var client = new Client(name);

        }
    }
}
