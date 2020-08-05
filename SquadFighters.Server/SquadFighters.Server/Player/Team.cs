using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquadFighters.Server {
    public class Team {

        public TeamName Name; //שם קבוצה
        public int PlayersCount; //כמות שחקנים בקבוצה
        public int CoinsCount; //כמות מטבעות בקבוצה

        /// <summary>
        /// פונקציה המקבלת שם קבוצה, כמות שחקנים, כמות מטבעות ויוצרת קבוצה
        /// </summary>
        /// <param name="name"></param>
        /// <param name="playersCount"></param>
        /// <param name="coinsCount"></param>
        public Team(TeamName name, int playersCount, int coinsCount) {
            Name = name;
            PlayersCount = playersCount;
            CoinsCount = coinsCount;
        }

        /// <summary>
        /// פונקציה המוסיפה כמות שחקנים לקבוצה
        /// </summary>
        public void AddPlayer() {
            PlayersCount++;
        }

        /// <summary>
        /// פונקציית הוספת מטבע
        /// </summary>
        public void AddCoin() {
            CoinsCount++;
        }

        /// <summary>
        /// פונקציה המקבלת מטבעות ומעדכנת את כמות המטבעות
        /// </summary>
        /// <param name="newCoins"></param>
        public void SetCoins(int newCoins) {
            CoinsCount = newCoins;
        }
    }
}
