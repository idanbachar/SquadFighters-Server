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

namespace SquadFighters.Server {
    public class Server {

        private TcpListener Listener; //מאזין לחיבורים
        private string ServerIp; //סרבר אייפי
        private int ServerPort; //סרבר פורט
        private Dictionary<string, Player> Clients; //מילון שחקנים
        private Dictionary<string, Team> Teams; //מילון קבוצות
        private string CurrentConnectedPlayerName; //שם של השחקן הנוכחי המחובר
        private Map Map; //מפה
        private string GameTitle; //שם משחק

        /// <summary>
        /// פונקציה המקבלת אייפי ופורט ויוצרת שרת
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public Server(string ip, int port) {
            ServerIp = ip;
            ServerPort = port;
            Clients = new Dictionary<string, Player>();
            Teams = new Dictionary<string, Team>();
            Teams.Add("Alpha", new Team(TeamName.Alpha, 0, 0));
            Teams.Add("Beta", new Team(TeamName.Beta, 0, 0));
            Teams.Add("Omega", new Team(TeamName.Omega, 0, 0));
            CurrentConnectedPlayerName = string.Empty;
            Map = new Map();
            GameTitle = "SquadFighters: BattleRoyale";
        }

        /// <summary>
        /// פונקציה המתחילה את השרת
        /// </summary>
        public void Start() {
            try {
                Listener = new TcpListener(IPAddress.Parse(ServerIp), ServerPort);
                Listener.Start();

                Console.WriteLine(GameTitle);
                Console.WriteLine("Server Started in " + ServerIp + ":" + ServerPort);

                //Load Map:
                Map.LoadItems();

                new Thread(WaitForConnections).Start();
                new Thread(Chat).Start();

            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// פונקציה המחכה לחיבורים מהקליינט
        /// </summary>
        public void WaitForConnections() {
            while (true) {
                Console.WriteLine("Waiting for connections..");
                TcpClient client = Listener.AcceptTcpClient();

                string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                new Thread(() => ReceiveDataFromClient(client)).Start();
                new Thread(SendPlayersInTeamsCount).Start();
                new Thread(SendCoinsInTeamsCount).Start();
            }
        }

        /// <summary>
        /// פונקציה המדפיסה מידע על השרת
        /// </summary>
        /// <param name="data"></param>
        public void Print(string data) {
            Console.WriteLine("<Server>: " + data);
        }

        /// <summary>
        /// פונקציה א-סינכרונית השולחת מידע של השחקנים/והמפה לכל שאר הקליינטים
        /// </summary>
        /// <param name="client"></param>
        public void SendItemsDataToClient(TcpClient client) {
            while (true) {
                try {
                    NetworkStream netStream = client.GetStream();
                    string itemsString = string.Empty;

                    foreach (KeyValuePair<string, string> item in Map.Items) {
                        itemsString = item.Value;

                        byte[] bytes = Encoding.ASCII.GetBytes(itemsString);
                        netStream.Write(bytes, 0, bytes.Length);
                        netStream.Flush();

                        Print("Sending item data to client -> " + itemsString);

                        Thread.Sleep(20);
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                }

                SendOneDataToClient(client, ServerMethod.MapDataDownloadCompleted.ToString());

                break;
            }
        }

        /// <summary>
        /// פונקציה המקבלת דאטה וקליינט ושולחת לו מידע
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        public void SendOneDataToClient(TcpClient client, string data) {
            try {
                NetworkStream netStream = client.GetStream();
                byte[] bytes = Encoding.ASCII.GetBytes(data);
                netStream.Write(bytes, 0, bytes.Length);
                netStream.Flush();

                Print(data);
            }
            catch (Exception) {

            }
        }

        /// <summary>
        /// פונקציה  המקבלת קליינט ומחזירה את שם השחקן
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public string GetPlayerNameByClient(TcpClient client) {
            foreach (KeyValuePair<string, Player> otherClient in Clients) {
                if (client == otherClient.Value.Client && !otherClient.Value.Client.Connected)
                    return otherClient.Value.Name;
            }

            return string.Empty;
        }

        /// <summary>
        /// פונקציה השולחת מידע על כמות שחקנים בכל קבוצה
        /// </summary>
        public void SendPlayersInTeamsCount() {
            while (true) {
                string message = ServerMethod.TeamsPlayersCounts.ToString() + "=true,Alpha=" + Teams[TeamName.Alpha.ToString()].PlayersCount + ",Beta=" + Teams[TeamName.Beta.ToString()].PlayersCount + ",Omega=" + Teams[TeamName.Omega.ToString()].PlayersCount;
                SendDataToAllClients(message);

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// פונקציה השולחת כמות מטבעות של כל קבוצה
        /// </summary>
        public void SendCoinsInTeamsCount() {
            while (true) {
                string message = ServerMethod.TeamsCoinsCount.ToString() + "=true,Alpha=" + Teams[TeamName.Alpha.ToString()].CoinsCount + ",Beta=" + Teams[TeamName.Beta.ToString()].CoinsCount + ",Omega=" + Teams[TeamName.Omega.ToString()].CoinsCount;
                SendDataToAllClients(message);

                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// פונקציה א-סינכרונית המקבלת קליינט ומקבלת את כל המידע עליו מהמשחק ומעדכנת בהתאם ושולחת נתונים לשאר השחקנים
        /// </summary>
        /// <param name="client"></param>
        public void ReceiveDataFromClient(TcpClient client) {
            while (true) {

                try {
                    NetworkStream netStream = client.GetStream();
                    byte[] bytes = new byte[1024];
                    netStream.Read(bytes, 0, bytes.Length);
                    string data = Encoding.ASCII.GetString(bytes);
                    string message = data.Substring(0, data.IndexOf("\0"));

                    if (message.Contains(ServerMethod.PlayerConnected.ToString())) {
                        CurrentConnectedPlayerName = message.Split(',')[0];
                        lock (Clients) {
                            Clients.Add(CurrentConnectedPlayerName, new Player(client, CurrentConnectedPlayerName));
                        }
                        SendDataToAllClients(message, client);

                        Print(CurrentConnectedPlayerName + " Connected to server.");
                        CurrentConnectedPlayerName = string.Empty;
                    }
                    else if (message == ServerMethod.StartDownloadMapData.ToString()) {
                        SendItemsDataToClient(client);
                    }
                    else if (message.Contains(ServerMethod.PlayerData.ToString())) {
                        // Print(message);
                        SendDataToAllClients(message, client);
                    }
                    else if (message.Contains(ServerMethod.ShootData.ToString())) {
                        Print(message);
                        SendDataToAllClients(message, client);
                    }
                    else if (message.Contains(ServerMethod.Revive.ToString())) {
                        Print(message);
                        SendDataToAllClients(message, client);
                    }
                    else if (message.Contains(ServerMethod.JoinedMatch.ToString())) {
                        string playerTeam = message.Split(',')[2];
                        Teams[playerTeam].AddPlayer();

                        Print(message);
                        SendDataToAllClients(message, client);
                    }
                    else if (message.Contains(ServerMethod.PlayerPopupMessage.ToString())) {
                        string popup = message.Split(',')[1].Split('=')[1];

                        Print(popup);
                        SendDataToAllClients(message);
                    }
                    else if (message.Contains(ServerMethod.PlayerKilled.ToString())) {
                        Print(message);
                        SendDataToAllClients(message, client);
                    }
                    else if (message.Contains(ServerMethod.PlayerDrown.ToString())) {
                        Print(message);
                        SendDataToAllClients(message, client);
                    }
                    else if (message.Contains(ServerMethod.UpdateSpawnerCoins.ToString())) {
                        Print(message);

                        if (message.Contains("AlphaTeam")) {
                            int coinsCount = int.Parse(message.Split(',')[1].Split('=')[1]);
                            Teams["Alpha"].SetCoins(coinsCount);
                        }
                        if (message.Contains("BetaTeam")) {
                            int coinsCount = int.Parse(message.Split(',')[1].Split('=')[1]);
                            Teams["Beta"].SetCoins(coinsCount);
                        }
                        if (message.Contains("OmegaTeam")) {
                            int coinsCount = int.Parse(message.Split(',')[1].Split('=')[1]);
                            Teams["Omega"].SetCoins(coinsCount);
                        }
                    }
                    else if (message.Contains(ServerMethod.ClientCreateItem.ToString())) {
                        Print(message);

                        int playerX = int.Parse(message.Split(',')[1].Split('=')[1]);
                        int playerY = int.Parse(message.Split(',')[2].Split('=')[1]);
                        int coinsCount = int.Parse(message.Split(',')[3].Split('=')[1]);
                        new Thread(() => CreateDroppedCoins(playerX, playerY, coinsCount)).Start();
                    }
                    else if (message.Contains(ServerMethod.RemoveItem.ToString())) {
                        string key = message.Split(',')[1];
                        lock (Map.Items) {
                            Map.Items.Remove(key);
                        }
                        SendDataToAllClients(message);
                        Print(message);
                    }
                    else if (message.Contains(ServerMethod.UpdateItemCapacity.ToString())) {
                        string receivedKey = message.Split(',')[2];
                        string receivedCapacityString = "Capacity=" + message.Split(',')[1];
                        lock (Map.Items) {
                            Map.Items[receivedKey].Split(',')[5] = receivedCapacityString;
                        }
                        SendDataToAllClients(message, client);
                        Print(message);
                    }

                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);

                    //DisconnectPlayer(client, GetPlayerNameByClient(client));
                }

                Thread.Sleep(20);
            }
        }

        /// <summary>
        /// פונקציה המקבלת מיקום של שחקן וכמות מטבעות ושולחת עדכון על נפילת מטבע
        /// </summary>
        /// <param name="playerX"></param>
        /// <param name="playerY"></param>
        /// <param name="coinsCount"></param>
        public void CreateDroppedCoins(int playerX, int playerY, int coinsCount) {
            for (int i = 0; i < coinsCount; i++) {
                ItemCategory itemToAdd = ItemCategory.Coin;
                Position coinPosition = new Position(playerX + 60 * i, playerY + 100 + new Random().Next(30, 60)); //GeneratePosition();
                CoinType coinType = CoinType.IB;
                string itemKey = itemToAdd.ToString() + "/" + coinType.ToString() + "/" + ((int)Map.Items.Count + 10 + new Random().Next(1000, 5001));
                string item = ServerMethod.DownloadDroppedCoins.ToString() + "=true,ItemCategory=" + (int)ItemCategory.Coin + ",CoinType=" + (int)coinType + ",X=" + coinPosition.X + ",Y=" + coinPosition.Y + ",Capacity=" + 25 + ",Key=" + itemKey + ",MaxItems=" + Map.MaxItems;
                Map.Items.Add(itemKey, item);

                SendDataToAllClients(item);
            }
        }

        /// <summary>
        /// צ'אט
        /// </summary>
        public void Chat() {

            while (true) {
                string message = Console.ReadLine();
                SendDataToAllClients(message);
                Console.WriteLine("<Server>: " + message);
            }
        }

        /// <summary>
        /// פונקציה המקבלת הודעה ושולחת לכל השחקנים
        /// </summary>
        /// <param name="message"></param>
        /// <param name="blackListedClient"></param>
        public void SendDataToAllClients(string message, TcpClient blackListedClient = null) {
            foreach (KeyValuePair<string, Player> player in Clients) {
                try {
                    NetworkStream netStream = player.Value.Client.GetStream();
                    byte[] bytes = Encoding.ASCII.GetBytes(message);

                    if (player.Value.Client != blackListedClient)
                        netStream.Write(bytes, 0, bytes.Length);

                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                    //DisconnectPlayer(player.Value.Client, player.Value.Name);
                }

                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// פונקציה המקבלת קליינט ומפתח למילון, ומוחקת את החיבור איתו
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        public void DisconnectPlayer(TcpClient client, string key) {
            client.Close();
            Clients.Remove(key);
            SendDataToAllClients(ServerMethod.PlayerDisconnected.ToString() + "=true,playerDisconnectedName=" + key);
        }
    }
}
