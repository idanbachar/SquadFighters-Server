using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquadFightersServer
{
    public class Team
    {
        public TeamName Name;
        public int PlayersCount;
        public int CoinsCount;

        public Team(TeamName name, int playersCount, int coinsCount)
        {
            Name = name;
            PlayersCount = playersCount;
            CoinsCount = coinsCount;
        }

        public void AddPlayer()
        {
            PlayersCount++;
        }

        public void AddCoin()
        {
            CoinsCount++;
        }

        public void SetCoins(int newCoins)
        {
            CoinsCount = newCoins;
        }
    }
}
