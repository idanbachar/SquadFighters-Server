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
        private Dictionary<string, Player> Clients;
        private string CurrentConnectedPlayerName;
        private Map Map;
        private string GameTitle;
        private ServerMethod ServerMethod;

        public Server(string ip, int port)
        {
            ServerIp = ip;
            ServerPort = port;
            Clients = new Dictionary<string, Player>();
            CurrentConnectedPlayerName = string.Empty;
            Map = new Map();
            GameTitle = "SquadFighters: BattleRoyale";
            ServerMethod = ServerMethod.None;
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

                        Print("Sending item data to client -> " + itemsString);

                        Thread.Sleep(20);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                SendOneDataToClient(client, ServerMethod.MapDataDownloadCompleted.ToString());
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

        public string GetPlayerNameByClient(TcpClient client)
        {
            foreach (KeyValuePair<string, Player> otherClient in Clients)
            {
                if (client == otherClient.Value.Client && !otherClient.Value.Client.Connected)
                    return otherClient.Value.Name;
            }

            return string.Empty;
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

                    if (message.Contains(ServerMethod.PlayerConnected.ToString()))
                    {
                        CurrentConnectedPlayerName = message.Split(',')[0];
                        lock (Clients)
                        {
                            Clients.Add(CurrentConnectedPlayerName, new Player(client, CurrentConnectedPlayerName));
                        }
                        SendDataToAllClients(message, client);

                        Print(CurrentConnectedPlayerName + " Connected to server.");
                        CurrentConnectedPlayerName = string.Empty;

                    }
                    else if (message == ServerMethod.StartDownloadMapData.ToString())
                    {
                        SendItemsDataToClient(client);
                    }
                    else if (message.Contains(ServerMethod.PlayerData.ToString()))
                    {
                        // Print(message);
                        SendDataToAllClients(message, client);
                    }
                    else if (message.Contains(ServerMethod.ShootData.ToString()))
                    {
                        Print(message);
                        SendDataToAllClients(message, client);
                    }
                    else if (message.Contains(ServerMethod.Revive.ToString()))
                    {
                        Print(message);
                        SendDataToAllClients(message, client);
                    }
                    else if (message.Contains(ServerMethod.JoinedMatch.ToString()))
                    {
                        Print(message);
                        SendDataToAllClients(message, client);
                    }
                    else if (message.Contains(ServerMethod.PlayerKilled.ToString()))
                    {
                        Print(message);
                        SendDataToAllClients(message, client);
                    }
                    else if (message.Contains(ServerMethod.RemoveItem.ToString()))
                    {
                        string key = message.Split(',')[1];
                        lock (Map.Items)
                        {
                            Map.Items.Remove(key);
                        }
                        SendDataToAllClients(message, client);
                        Print(message);
                    }
                    else if (message.Contains(ServerMethod.UpdateItemCapacity.ToString()))
                    {
                        string receivedKey = message.Split(',')[2];
                        string receivedCapacityString = "Capacity=" + message.Split(',')[1];
                        lock (Map.Items)
                        {
                            Map.Items[receivedKey].Split(',')[5] = receivedCapacityString;
                        }
                        SendDataToAllClients(message, client);
                        Print(message);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    DisconnectPlayer(client, GetPlayerNameByClient(client));
                }

                Thread.Sleep(20);
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
            foreach (KeyValuePair<string, Player> player in Clients)
            {
                try
                {
                    NetworkStream netStream = player.Value.Client.GetStream();
                    byte[] bytes = Encoding.ASCII.GetBytes(message);

                    if (player.Value.Client != blackListedClient)
                        netStream.Write(bytes, 0, bytes.Length);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    //DisconnectPlayer(player.Value.Client, player.Value.Name);
                }

                Thread.Sleep(10);
            }
        }

        public void DisconnectPlayer(TcpClient client, string key)
        {
            client.Close();
            Clients.Remove(key);
            SendDataToAllClients(ServerMethod.PlayerDisconnected.ToString() + "=true,playerDisconnectedName=" + key);
        }
    }
}
