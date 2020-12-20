using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UNOServer.ServerObjects;

namespace UNOServer {
    class Program {

        static ServerObject server;
        static TcpListener tcpListener;
        // главный клиент, который начинает игру
        static ClientObject mainClient;
        // список клиентов для подключения
       
        static bool started = false;

        static void Main(string[] args) {
            try {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");
 
                server = new ServerObject(tcpListener);

                //TcpClient tcpClient = tcpListener.AcceptTcpClient();
                //mainClient = new ClientObject(tcpClient, server);

                //Console.WriteLine($"Подключен раздающий игрок: {mainClient.Player.Name}");
                //mainClient.Player = new GameObjects.Player(message);

                new Thread(new ThreadStart(Listen)).Start();

                //while (!started) {
                //    string message = RecieveMessage(mainClient.Stream);
                //    started = message == "start";
                //}
                Console.ReadLine();
                started = true;
                Console.WriteLine("Игра началась");
                server.Play();

            } catch (Exception ex) {
                Console.WriteLine("Подключение прервано!"); //соединение было прервано
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                Disconnect();
            }
        }

        private static void Listen() {
            while (!started) {
                var tcpClient = tcpListener.AcceptTcpClient();  
                if (!started) {
                    var client = new ClientObject(tcpClient, server);
                    //Thread clientThread = new Thread(new ThreadStart(client.Process));
                    //clientThread.Start();
                }
            }
        }

        static string RecieveMessage(NetworkStream stream) {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            return builder.ToString();
        }

        // отключение всех клиентов
        static void Disconnect() {
            tcpListener.Stop(); //остановка сервера
            server.Disconnect();
        //    for (int i = 0; i < clients.Count; i++) {
        //        clients[i].Close(); //отключение клиента
        //    }
            Environment.Exit(0); //завершение процесса
        }
    }
}
