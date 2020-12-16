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
        static TcpClient mainClient;

        static void Main(string[] args) {
            try {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");
                // список клиентов для подключения
                var lobby = new List<TcpClient>();
                // клиент, который начинает игру

                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                mainClient = tcpClient;

                waitThread = new Thread(new ThreadStart(WaitUntilStart));
                waitThread.Start();

                while (true) {
                    tcpClient = tcpListener.AcceptTcpClient();
                    lobby.Add(tcpClient);
                    //ClientObject clientObject = new ClientObject(tcpClient, this);
                    //Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    //clientThread.Start();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                //Disconnect();
            }
        }

        private static void WaitUntilStart() {
            var stream = mainClient.GetStream();
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
                    Console.WriteLine(message);//вывод сообщения
                } catch {
                    Console.WriteLine("Подключение прервано!"); //соединение было прервано
                    Console.ReadLine();
                    //Disconnect();
                }
            }
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
