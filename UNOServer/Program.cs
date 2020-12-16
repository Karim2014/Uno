using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UNOServer.ServerObjects;

namespace UNOServer {
    class Program {

        static TcpListener tcpListener;
        static Thread waitThread;
        // главный клиент, который начинает игру
        static ClientObject mainClient;
        // список клиентов для подключения
        static List<ClientObject> lobby = new List<ClientObject>();

        static void Main(string[] args) {
            try {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                mainClient = new ClientObject(tcpClient);
                lobby.Add(mainClient);
                Console.WriteLine("Подключен раздающий игрок");

                waitThread = new Thread(new ThreadStart(Listen));
                waitThread.Start();

                var stream = mainClient.Stream;
                while (true) {
                    try {
                        byte[] data = new byte[64]; // буфер для получаемых данных
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do {
                            bytes = stream.Read(data, 0, data.Length);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        string message = builder.ToString();
                        //Console.WriteLine(message);//вывод сообщения
                        if (message == "start") {
                            waitThread.Abort();
                            Console.WriteLine("Игра началась");
                            break;
                        }
                    } catch {
                        Console.WriteLine("Подключение прервано!"); //соединение было прервано
                        Console.ReadLine();
                        //Disconnect();
                    }
                }

            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                //Disconnect();
            }
        }

        private static void Listen() {
            while (true) {
                var tcpClient = tcpListener.AcceptTcpClient();
                lobby.Add(new ClientObject(tcpClient));
                Console.WriteLine("Подключен игрок");
                //ClientObject clientObject = new ClientObject(tcpClient, this);
                //Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                //clientThread.Start();
            }
        }

        static void RecieveMessage() {

        }

        // отключение всех клиентов
        //static void Disconnect() {
        //    tcpListener.Stop(); //остановка сервера

        //    for (int i = 0; i < clients.Count; i++) {
        //        clients[i].Close(); //отключение клиента
        //    }
        //    Environment.Exit(0); //завершение процесса
        //}
    }
}
