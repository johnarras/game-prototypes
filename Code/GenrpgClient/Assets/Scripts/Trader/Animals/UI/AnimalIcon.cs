using Assets.Scripts.Entities.UI;
using Genrpg.Shared.Trader.Animals.Settings;

namespace Assets.Scripts.Trader.UI.Animals
{
    public enum EAnimalLocations
    {
        Caravan = 0,
        Holdings = 1,
        Vendor = 2,
    }

    public class AnimalIconInitData
    {
        public AnimalType AnimalType { get; set; }
        public EAnimalLocations Location { get; set; }
        public long Cost;
    }


    public class AnimalIcon : EntityIcon
    {
        public GText Name;
        public GText Speed;
        public GText Capacity;
        public GText Upkeep;
        public GText Price;

        public GButton RemoveButton;
        public GButton BuyButton;
        public GButton AddButton;


        public void SetData(AnimalIconInitData init)
        {

        }

    }
}
