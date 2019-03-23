using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquadFightersServer
{
    public class Map
    {
        public Dictionary<string, string> Items;
        private Random Random;
        public int Width;
        public int Height;
        public int MaxItems;

        public Map()
        {
            Items = new Dictionary<string, string>();
            Random = new Random();
            Width = 5000;
            Height = 5000;
            MaxItems = 180;
        }

        public void LoadItems()
        {
            Random rndItem = new Random();

            for (int i = 0; i < 160; i++)
                AddItem((ItemCategory)rndItem.Next(3));

            for(int i = 0; i < 20; i++)
            {
                AddItem(ItemCategory.Coin);
            }
        }

        public void AddCoinsTest()
        {
            for (int i = 0; i < 5; i++)
            {
                ItemCategory itemToAdd = ItemCategory.Coin;
                Position coinPosition = new Position(100 + 40 * i, 500); //GeneratePosition();
                CoinType coinType = CoinType.IB;
                string itemKey = itemToAdd.ToString() + "/" + coinType.ToString() + "/" + Items.Count;
                string item = ServerMethod.DownloadDroppedCoins.ToString() + "=true,ItemCategory=" + (int)ItemCategory.Coin + ",CoinType=" + (int)coinType + ",X=" + coinPosition.X + ",Y=" + coinPosition.Y + ",Capacity=" + 25 + ",Key=" + itemKey + ",MaxItems=" + MaxItems;
                Items.Add(itemKey, item);
            }
        }

        public void AddItem(ItemCategory itemToAdd)
        {
            string item = string.Empty;
            string itemKey = string.Empty;

            switch (itemToAdd)
            {
                case ItemCategory.Ammo:
                    Position ammoPosition = GeneratePosition();
                    AmmoType ammoType = GenerateAmmo();
                    itemKey = itemToAdd.ToString() + "/" + ammoType.ToString() + "/" + Items.Count;
                    item = ServerMethod.DownloadingItem.ToString() + "=true,ItemCategory=" + (int)ItemCategory.Ammo + ",AmmoType=" + (int)ammoType + ",X=" + ammoPosition.X + ",Y=" + ammoPosition.Y + ",Capacity=" + GenerateCapacity(itemToAdd) + ",Key=" + itemKey + ",MaxItems=" + MaxItems;
                    Items.Add(itemKey,item);
                    break;
                case ItemCategory.Food:
                    Position foodPosition = GeneratePosition();
                    FoodType foodType = GenerateFood();
                    itemKey = itemToAdd.ToString() + "/" + foodType.ToString() + "/" + Items.Count;
                    item = ServerMethod.DownloadingItem.ToString() + "=true,ItemCategory=" + (int)ItemCategory.Food + ",FoodType=" + (int)foodType + ",X=" + foodPosition.X + ",Y=" + foodPosition.Y + ",Capacity=" + GenerateCapacity(itemToAdd) + ",Key=" + itemKey + ",MaxItems=" + MaxItems;
                    Items.Add(itemKey, item);
                    break;
                case ItemCategory.Shield:
                    Position shieldPosition = GeneratePosition();
                    ShieldType shieldType = GenerateShield();
                    itemKey = itemToAdd.ToString() + "/" + shieldType.ToString() + "/" + Items.Count;
                    item = ServerMethod.DownloadingItem.ToString() + "=true,ItemCategory=" + (int)ItemCategory.Shield + ",ShieldType=" + (int)shieldType + ",X=" + shieldPosition.X + ",Y=" + shieldPosition.Y + ",Capacity=" + 100 + ",Key=" + itemKey + ",MaxItems=" + MaxItems;
                    Items.Add(itemKey, item);
                    break;
                case ItemCategory.Helmet:
                    Position helmetPosition = GeneratePosition();
                    HelmetType helmetType = GenerateHelmet();
                    itemKey = itemToAdd.ToString() + "/" + helmetType.ToString() + "/" + Items.Count;
                    item = ServerMethod.DownloadingItem.ToString() + "=true,ItemCategory=" + (int)ItemCategory.Helmet + ",HelmetType=" + (int)helmetType + ",X=" + helmetPosition.X + ",Y=" + helmetPosition.Y + ",Capacity=" + 100 + ",Key=" + itemKey + ",MaxItems=" + MaxItems;
                    Items.Add(itemKey, item);
                    break;
                case ItemCategory.Coin:
                    Position coinPosition = GeneratePosition();
                    CoinType coinType = CoinType.IB;
                    itemKey = itemToAdd.ToString() + "/" + coinType.ToString() + "/" + Items.Count;
                    item = ServerMethod.DownloadingItem.ToString() + "=true,ItemCategory=" + (int)ItemCategory.Coin + ",CoinType=" + (int)coinType + ",X=" + coinPosition.X + ",Y=" + coinPosition.Y + ",Capacity=" + 25 + ",Key=" + itemKey + ",MaxItems=" + MaxItems;
                    Items.Add(itemKey, item);
                    break;
            }
        }

        public int GenerateCapacity(ItemCategory itemCategory)
        {
            switch (itemCategory)
            {
                case ItemCategory.Ammo:
                    return Random.Next(7, 27);
                case ItemCategory.Food:
                    return Random.Next(30, 74);
                default:
                    return 20;
            }
        }

        public ShieldType GenerateShield()
        {
            int Number = Random.Next(1000);

            if (Number >= 0 && Number <= 400)
                return ShieldType.Shield_Level_1;
            else if (Number >= 401 && Number <= 700)
                return ShieldType.Shield_Level_2;
            else if (Number >= 701 && Number <= 900)
                return ShieldType.Shield_Rare;
            else if (Number >= 901 && Number <= 1000)
                return ShieldType.Shield_Legendery;

            return ShieldType.Shield_Legendery;
        }

        public HelmetType GenerateHelmet() { return (HelmetType)(Random.Next(4)); }
        public AmmoType GenerateAmmo() { return (AmmoType)(Random.Next(1, 2)); }
        public FoodType GenerateFood() { return (FoodType)(Random.Next(3)); }
        public Position GeneratePosition() { return new Position(Random.Next(200, Width - 200), Random.Next(200, Height - 200)); }

    }
}
