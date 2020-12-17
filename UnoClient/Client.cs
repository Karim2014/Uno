using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UnoClient {
    class Client {

        private string userName;
        private const string host = "127.0.0.1";
        private const int port = 8888;

        private TcpClient client;
        private NetworkStream stream;

        public Client(string name) {
            userName = name;
            client = new TcpClient();

            try {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток

                string message = userName;
                Send(message);

                // запускаем новый поток для получения данных
                new Thread(new ThreadStart(ReceiveMessage)).Start();

                Console.WriteLine("Добро пожаловать, {0}", userName);
                SendMessage();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            } finally {
                Disconnect();
            }
        }

        // отправка сообщений
        private void SendMessage() {
            Console.WriteLine("Введите сообщение: ");

            while (true) {
                string message = Console.ReadLine();
                Send(message);
            }
        }

        private void Send(string message) {
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        // получение сообщений
        private void ReceiveMessage() {
            while (true) {
                try {
                    string message = GetMessage();
                    string[] m = message.Split(':');
                    Console.WriteLine(message);
                    Console.WriteLine($"head: <{m[0]}> body: <{m[1]}>");
                    ProcessMessage(m[0], m[1]);

                } catch (Exception e) {
                    Console.WriteLine("Подключение прервано!"); //соединение было прервано
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        private string GetMessage() {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            do {
                int bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            return builder.ToString();
        }

        private void ProcessMessage(string head, string body) {
            switch (head) {
                case "text":
                    Console.WriteLine(body);
                    return;
                case "cards":
                    Console.WriteLine(body);
                    return;
            }
        }

        private void Disconnect() {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }

    }
}
