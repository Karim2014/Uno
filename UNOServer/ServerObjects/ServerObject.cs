using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UNOServer.GameObjects;

namespace UNOServer.ServerObjects {

    public class ServerObject {

        static TcpListener tcpListener; // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения
        Game game;

        protected internal void AddConnection(ClientObject clientObject) {
            clients.Add(clientObject);
        }

        internal void BroadcastMessage(string message, string id) {
            throw new NotImplementedException();
        }

        // прослушивание входящих подключений
        protected internal void Listen() {
            try {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true) {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    //Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    //clientThread.Start();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal void Play() {

        }

        protected internal void RemoveConnection(string id) {
            // получаем по id закрытое подключение
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }

        // отключение всех клиентов
        protected internal void Disconnect() {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++) {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }
    }
}
