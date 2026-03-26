using System.Collections.Generic;

namespace Genrpg.Shared.PlayerGroups.Entities
{
    public class PlayerGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int GroupType { get; set; }

        public List<GroupMember> Members { get; set; }

        public PlayerGroup()
        {
            Members = new List<GroupMember>();
        }
    }
}


