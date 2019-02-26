using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SquadFightersServer
{
    public class Server
    {
        private TcpListener listener;
        private string serverIp;
        private int serverPort;
        private Dictionary<string, TcpClient> clients;
        private string CurrentConnectedPlayerName;

        public Server(string ip, int port)
        {
            this.serverIp = ip;
            this.serverPort = port;
            this.clients = new Dictionary<string, TcpClient>();
            CurrentConnectedPlayerName = string.Empty;
        }

        public void Start()
        {

            try
            {
                listener = new TcpListener(IPAddress.Parse(serverIp), serverPort);
                listener.Start();

                Console.WriteLine("Server started on ip '" + serverIp + "' and port '" + serverPort + ".");

                new Thread(WaitForConnections).Start();
                new Thread(Chat).Start();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void WaitForConnections()
        {

            while (true)
            {

                Console.WriteLine("Waiting for connections..");
                TcpClient client = listener.AcceptTcpClient();
                string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                if (!clients.ContainsKey(CurrentConnectedPlayerName))
                {
                    AddConnectedPlayer(client);
                    new Thread(() => Recieve(client)).Start();
                }
                else
                {
                    Console.WriteLine("You are already connected to the server!");
                }

            }

        }

        public void AddConnectedPlayer(TcpClient client)
        {
            try
            {
                NetworkStream netStream = client.GetStream();
                byte[] bytes = new byte[1024];
                netStream.Read(bytes, 0, bytes.Length);
                string data = Encoding.ASCII.GetString(bytes);
                string message = data.Substring(0, data.IndexOf("\0"));

                if (message.Contains("Connected"))
                {
                    CurrentConnectedPlayerName = message.Split(',')[0];
                    clients.Add(CurrentConnectedPlayerName, client);
                    Console.WriteLine("<Client>: " + CurrentConnectedPlayerName + " Connected.");
                    CurrentConnectedPlayerName = string.Empty;
                }

                SendAll(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Recieve(TcpClient client)
        {

            while (true)
            {

                try
                {

                    NetworkStream netStream = client.GetStream();
                    byte[] bytes = new byte[1024];
                    netStream.Read(bytes, 0, bytes.Length);
                    string data = Encoding.ASCII.GetString(bytes);
                    string message = data.Substring(0, data.IndexOf("\0"));

                    SendAll(message, client);

                    if (message.Contains("Connected"))
                    {
                        CurrentConnectedPlayerName = message.Split(',')[0];
                        clients.Add(CurrentConnectedPlayerName, client);
                    }

                    if (message != "")
                        Console.WriteLine("<Client>: " + message);
                    else
                    {
                        clients.Remove(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
                    }
                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void Chat()
        {

            while (true)
            {

                string message = Console.ReadLine();
                SendAll(message);
                Console.WriteLine("<Server>: " + message);
            }
        }

        public void SendAll(string message, TcpClient blackListedClient = null)
        {
            try
            {
                foreach (KeyValuePair<string, TcpClient> client in clients)
                {
                    NetworkStream netStream = client.Value.GetStream();
                    byte[] bytes = Encoding.ASCII.GetBytes(message);

                    if(client.Value != blackListedClient)
                        netStream.Write(bytes, 0, bytes.Length);

                    Thread.Sleep(50);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void SendTo(TcpClient client, string message)
        {

            NetworkStream netStream = client.GetStream();
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            netStream.Write(bytes, 0, bytes.Length);

        }

        public byte[] ObjectToByteArray(Object obj)
        {

            if (obj == null)
                return null;

            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            byte[] arr = stream.ToArray();
            stream.Close();
            return arr;
        }

        public object ByteArrayToObject(byte[] Buffer)
        {

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(Buffer);
            object obj = null;
            try
            {
                obj = formatter.Deserialize(stream);
            }
            catch
            {
                obj = null;
            }
            stream.Close();
            return obj;
        }
    }
}
