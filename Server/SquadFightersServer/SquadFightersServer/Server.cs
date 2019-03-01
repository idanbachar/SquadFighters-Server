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
        private TcpListener Listener;
        private string ServerIp;
        private int ServerPort;
        private Dictionary<string, TcpClient> Clients;
        private string CurrentConnectedPlayerName;
        private Map Map;
        private string GameTitle;

        public Server(string ip, int port)
        {
            ServerIp = ip;
            ServerPort = port;
            Clients = new Dictionary<string, TcpClient>();
            CurrentConnectedPlayerName = string.Empty;
            Map = new Map();
            GameTitle = "SquadFighters: BattleRoyale";
        }

        public void Start()
        {
            try
            {
                Listener = new TcpListener(IPAddress.Parse(ServerIp), ServerPort);
                Listener.Start();
                Console.WriteLine("Server Started in " + ServerIp + ":" + ServerPort);

                //Load Map:
                Map.Load();
                 
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
                TcpClient client = Listener.AcceptTcpClient();
                string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                new Thread(() => ReceiveDataFromClient(client)).Start();
 
            }
        }

        public void Print(string data)
        {
            Console.WriteLine("<Server>: " + data);
        }

        public string GetPlayerNameByClient(TcpClient client)
        {
            foreach(KeyValuePair<string, TcpClient> otherClient in Clients)
            {
                if (client == otherClient.Value) return otherClient.Key;
            }
            return string.Empty;
        }

        public void SendItemsDataToClient(TcpClient client)
        {
            while (true)
            {
                try
                {
                    NetworkStream netStream = client.GetStream();
                    string itemsString = string.Empty;
                    foreach (KeyValuePair<string, string> item in Map.Items)
                    {
                        itemsString = item.Value;

                        byte[] bytes = Encoding.ASCII.GetBytes(itemsString);
                        netStream.Write(bytes, 0, bytes.Length);
                        netStream.Flush();

                        Print("Sending items data to " + GetPlayerNameByClient(client) + " -> " + itemsString);

                        Thread.Sleep(50);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                SendOneDataToClient(client,"Load Items Completed");
                break;
            }
        }

        public void SendOneDataToClient(TcpClient client, string data)
        {
            try
            {
                NetworkStream netStream = client.GetStream();
                byte[] bytes = Encoding.ASCII.GetBytes(data);
                netStream.Write(bytes, 0, bytes.Length);
                netStream.Flush();

                Print(data);
            }
            catch (Exception)
            {

            }
        }

        public void ReceiveDataFromClient(TcpClient client)
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

                    if (message.Contains("Connected"))
                    {
                        CurrentConnectedPlayerName = message.Split(',')[0];
                        Clients.Add(CurrentConnectedPlayerName, client);
                        Print(CurrentConnectedPlayerName + " has connected to '" + GameTitle + "' server!");
                        Clients.Add(CurrentConnectedPlayerName, client);
                        SendDataToAllClients(message, client);

                        CurrentConnectedPlayerName = string.Empty;

                    }
                    else if(message.Contains("Load Map"))
                    {
                        SendItemsDataToClient(client);
                    }
                    else if (message.Contains("PlayerName"))
                    {
                        SendDataToAllClients(message, client);
                    }
                    if (message.Contains("Remove Item"))
                    {
                        string key = message.Split(',')[1];
                        Map.Items.Remove(key);
                        SendDataToAllClients(message, client);
                    }
                    if (message.Contains("Update Item Capacity"))
                    {
                        string receivedKey = message.Split(',')[2];
                        string receivedCapacityString = "Capacity=" + message.Split(',')[1];

                        Map.Items[receivedKey].Split(',')[5] = receivedCapacityString;
                        SendDataToAllClients(message, client);                       
                    }

                    Thread.Sleep(70);
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
                SendDataToAllClients(message);
                Console.WriteLine("<Server>: " + message);
            }
        }

        public void SendDataToAllClients(string message, TcpClient blackListedClient = null)
        {
            try
            {
                foreach (KeyValuePair<string, TcpClient> client in Clients)
                {
                    NetworkStream netStream = client.Value.GetStream();
                    byte[] bytes = Encoding.ASCII.GetBytes(message);

                    if(client.Value != blackListedClient)
                        netStream.Write(bytes, 0, bytes.Length);


                    netStream.Flush();
                    Thread.Sleep(50);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
