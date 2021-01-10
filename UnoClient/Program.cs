using System;
using System.Net;

namespace UnoClient {
    class Program {

        static void Main(string[] args) {

            Console.Write("Добро пожаловать в игру UNO. Представтесь, пожалуйста: ");
            string name = Console.ReadLine();
            var client = new Client(name);

            bool connected = false;
            do {
                Console.Write($"{name}, введите адрес сервера: ");
                IPAddress ipAddress;
                while (!IPAddress.TryParse(Console.ReadLine(), out ipAddress))    
                    Console.Write("Введите корректный адрес: ");
                if (!(connected = client.Connect(ipAddress)))
                    Console.WriteLine("Не удалось подключиться к серверу. Повторите попытку");
            } while (!connected);

            client.Play();
        }
    }
}
