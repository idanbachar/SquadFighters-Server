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
        private int MaxItems;

        public Map()
        {
            Items = new Dictionary<string, string>();
            Random = new Random();
            Width = 4000;
            Height = 4000;
            MaxItems = Width / 10;
        }

        public void Load()
        {
            Random rndItem = new Random();

            for (int i = 0; i < MaxItems; i++)
                AddItem((ItemCategory)rndItem.Next(4));
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
                    item = "AddItem=true,ItemCategory=" + (int)ItemCategory.Ammo + ",AmmoType=" + (int)ammoType + ",X=" + ammoPosition.X + ",Y=" + ammoPosition.Y + ",Capacity=" + GenerateCapacity(itemToAdd) + ",Key=" + itemKey + ",MaxItems=" + MaxItems;
                    Items.Add(itemKey,item);
                    break;
                case ItemCategory.Food:
                    Position foodPosition = GeneratePosition();
                    FoodType foodType = GenerateFood();
                    itemKey = itemToAdd.ToString() + "/" + foodType.ToString() + "/" + Items.Count;
                    item = "AddItem=true,ItemCategory=" + (int)ItemCategory.Food + ",FoodType=" + (int)foodType + ",X=" + foodPosition.X + ",Y=" + foodPosition.Y + ",Capacity=" + GenerateCapacity(itemToAdd) + ",Key=" + itemKey + ",MaxItems=" + MaxItems;
                    Items.Add(itemKey, item);
                    break;
                case ItemCategory.Shield:
                    Position shieldPosition = GeneratePosition();
                    ShieldType shieldType = GenerateShield();
                    itemKey = itemToAdd.ToString() + "/" + shieldType.ToString() + "/" + Items.Count;
                    item = "AddItem=true,ItemCategory=" + (int)ItemCategory.Shield + ",ShieldType=" + (int)shieldType + ",X=" + shieldPosition.X + ",Y=" + shieldPosition.Y + ",Capacity=" + 100 + ",Key=" + itemKey + ",MaxItems=" + MaxItems;
                    Items.Add(itemKey, item);
                    break;
                case ItemCategory.Helmet:
                    Position helmetPosition = GeneratePosition();
                    HelmetType helmetType = GenerateHelmet();
                    itemKey = itemToAdd.ToString() + "/" + helmetType.ToString() + "/" + Items.Count;
                    item = "AddItem=true,ItemCategory=" + (int)ItemCategory.Helmet + ",HelmetType=" + (int)helmetType + ",X=" + helmetPosition.X + ",Y=" + helmetPosition.Y + ",Capacity=" + 100 + ",Key=" + itemKey + ",MaxItems=" + MaxItems;
                    Items.Add(itemKey, item);
                    break;
            }
        }

        public ShieldType GenerateShield()
        {
            int Number = Random.Next(71);

            if (Number >= 0 && Number <= 10)
                return ShieldType.Shield_Level_1;
            else if (Number >= 11 && Number <= 20)
                return ShieldType.Shield_Level_2;
            else if (Number >= 21 && Number <= 30)
                return ShieldType.Shield_Rare;
            else
                return ShieldType.Shield_Legendery;
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

        public HelmetType GenerateHelmet() { return (HelmetType)(Random.Next(4)); }
        public AmmoType GenerateAmmo() { return (AmmoType)(Random.Next(1, 2)); }
        public FoodType GenerateFood() { return (FoodType)(Random.Next(3)); }
        public Position GeneratePosition() { return new Position(Random.Next(200, Width - 200), Random.Next(200, Height - 200)); }

    }
}
