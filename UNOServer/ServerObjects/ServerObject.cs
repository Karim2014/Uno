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

        public ServerObject(TcpListener listener) {
            tcpListener = listener;
        }

        protected internal void Play() {
            game = new Game(clients.Select(client => client.Player).ToList(), this);
            game.PlayGame();
        }

        protected internal void AddConnection(ClientObject clientObject) {
            clients.Add(clientObject);
        }

        protected internal void BroadcastMessage(string message) {
            byte[] data = Encoding.Unicode.GetBytes(message);
            Thread.Sleep(100);
            clients.ForEach(client => client.Stream.Write(data, 0, data.Length));
        }

        internal void BroadcastMessage(string message, string id) {
            Thread.Sleep(100);
            for (int i = 0; i < clients.Count; i++) {
                if (clients[i].Id != id) // если id клиента не равно id отправляющего
                {
                    TargetMessage(message, clients[i]);
                }
            }
        }

        protected internal string GetMessageFromPlayer(string message, Player player) {
            ClientObject client = clients.FirstOrDefault(cl => cl.Id == player.Id);
            if (client != null) {
                TargetMessage("cmd^" + message, client);
                return client.GetMessage();
            }
            return "";
        }

        protected internal void TargetMessage(string message, ClientObject client) {
            if (client != null) {
                Thread.Sleep(100);
                byte[] data = Encoding.Unicode.GetBytes(message);
                client.Stream.Write(data, 0, data.Length);
            }
        }

        protected internal void TargetMessage(string message, Player player) {
            ClientObject client = clients.FirstOrDefault(cl => cl.Player.Id == player.Id);
            TargetMessage(message, client);
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
