using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UNOServer.GameObjects;

namespace UNOServer.ServerObjects {
    public class ClientObject {

        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        protected internal Player Player { get; set; }

        private TcpClient client;
        private ServerObject server;

        private BinaryReader reader;
        private BinaryWriter writer;

        public ClientObject(TcpClient tcpClient) {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
        }

        public ClientObject(TcpClient tcpClient, ServerObject serverObject) {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
            Stream = client.GetStream();
            reader = new BinaryReader(Stream);
            writer = new BinaryWriter(Stream);

            // получаем имя пользователя
            string message = GetMessage();
            Player = new Player(message) { Id = Id };
            message = Player.Name + " подключился к игре";
            // посылаем сообщение о входе в чат всем подключенным пользователям
            server.BroadcastMessage(message, Player);
            Console.WriteLine(message);
        }

        public void Process() {
            try {

                string message;
                
                // в бесконечном цикле получаем сообщения от клиента
                while (true) {
                    try {
                        message = GetMessage();
                        message = String.Format("{0}: {1}", Player.Name, message);
                        Console.WriteLine(message);
                        //server.BroadcastMessage(message, this.Id);
                    } catch {
                        message = String.Format("{0}: покинул чат", Player.Name);
                        Console.WriteLine(message);
                        //server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            } finally {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        public string GetMessage() {
            return reader.ReadString();
        }

        public void SendMessage(string message) {
            writer.Write(message);
            writer.Flush();
        }

        // закрытие подключения
        protected internal void Close() {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
            if (reader != null)
                reader.Close();
            if (writer != null)
                writer.Close();
        }

    }
}
