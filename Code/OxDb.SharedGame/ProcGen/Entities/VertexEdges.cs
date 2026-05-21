using System.Collections.Generic;

namespace OxDb.SharedGame.ProcGen.Entities
{
    public class VertexEdges
    {
        public int Id { get; set; }
        public int PosId { get; set; }
        public int X { get; set; }

        public int Y { get; set; }

        public List<int> AdjacentVerts { get; set; }

        public VertexEdges()
        {
            AdjacentVerts = new List<int>();
        }
    }
}


