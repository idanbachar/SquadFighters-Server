using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquadFightersServer
{
    public class Map
    {
        public List<string> Items;
        private Random Random;
        public int Width;
        public int Height;

        public Map()
        {
            Items = new List<string>();
            Random = new Random();
            Width = 10000;
            Height = 10000;
        }

        public void AddItem(ItemCategory itemToAdd)
        {
            string item = string.Empty;

            switch (itemToAdd)
            {
                case ItemCategory.Ammo:
                    Position ammoPosition = GeneratePosition();
                    item = "AddItem=true,ItemCategory=" + (int)ItemCategory.Ammo + ",AmmoType=" + (int)GenerateAmmo() + ",X=" + ammoPosition.X + ",Y=" + ammoPosition.Y;
                    Items.Add(item);
                    break;
                case ItemCategory.Food:
                    Position foodPosition = GeneratePosition();
                    item = "AddItem=true,ItemCategory=" + (int)ItemCategory.Food + ",FoodType=" + (int)GenerateFood() + ",X=" + foodPosition.X + ",Y=" + foodPosition.Y;
                    Items.Add(item);
                    break;
                case ItemCategory.Shield:
                    Position shieldPosition = GeneratePosition();
                    item = "AddItem=true,ItemCategory=" + (int)ItemCategory.Shield + ",ShieldType=" + (int)GenerateShield() + ",X=" + shieldPosition.X + ",Y=" + shieldPosition.Y;
                    Items.Add(item);
                    break;
                case ItemCategory.Helmet:
                    Position helmetPosition = GeneratePosition();
                    item = "AddItem=true,ItemCategory=" + (int)ItemCategory.Helmet + ",HelmetType=" + (int)GenerateHelmet() + ",X=" + helmetPosition.X + ",Y=" + helmetPosition.Y;
                    Items.Add(item);
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

        public HelmetType GenerateHelmet() { return (HelmetType)(Random.Next(4)); }
        public AmmoType GenerateAmmo() { return (AmmoType)(Random.Next(1, 2)); }
        public FoodType GenerateFood() { return (FoodType)(Random.Next(3)); }
        public Position GeneratePosition() { return new Position(Random.Next(200, Width - 200), Random.Next(200, Height - 200)); }

    }
}
