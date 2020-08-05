using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SquadFighters.Server {
    public class Player {

        public TcpClient Client; //קליינט
        public string Name; //שם שחקן

        /// <summary>
        /// פונקציה המקבלת קליינט ושם, ומייצרת שחקן
        /// </summary>
        /// <param name="client"></param>
        /// <param name="name"></param>
        public Player(TcpClient client, string name) {
            Client = client;
            Name = name;
        }
    }
}
