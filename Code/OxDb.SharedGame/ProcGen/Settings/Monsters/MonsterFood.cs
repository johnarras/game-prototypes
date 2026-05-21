namespace OxDb.SharedGame.ProcGen.Settings.Monsters
{
    /// <summary>
    /// Set up what this creature likes to eat. It can be plants or CaravanMembers or both.
    /// </summary>

    public class MonsterFood
    {
        /// <summary>
        /// What entity type the desired food is
        /// </summary>
        public long FoodEntityTypeId { get; set; }

        /// <summary>
        /// What the entity key is for this food type.
        /// </summary>
        public int FoodEntityId { get; set; }
        public string Name { get; set; }

    }
}


