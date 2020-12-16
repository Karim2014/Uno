using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UNOServer.ServerObjects;

namespace UNOServer {
    class Program {
        static ServerObject server; // сервер
        static Thread listenThread; // потока для прослушивания
        static TcpListener tcpListener;
        static void Main(string[] args) {
            try {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true) {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    
                    //ClientObject clientObject = new ClientObject(tcpClient, this);
                    //Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    //clientThread.Start();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                //Disconnect();
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
