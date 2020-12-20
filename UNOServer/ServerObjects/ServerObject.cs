using System;
using System.Collections.Generic;
using System.IO;
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
            clients.ForEach(client => client.SendMessage(message));
        }

        internal void BroadcastMessage(string message, Player exceptPlayer) {
            clients
                .Where(client => client.Id != exceptPlayer.Id)
                .ToList()
                .ForEach(client => client.SendMessage(message));
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
                client.SendMessage(message);
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
