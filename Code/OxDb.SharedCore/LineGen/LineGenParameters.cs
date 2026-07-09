namespace OxDb.SharedCore.LineGen
{
    public class LineGenParameters
    {
        public int WidthSize { get; set; }
        public int MinWidthSize { get; set; }
        public int MaxWidthSize { get; set; }
        public int WidthSizeChangeAmount { get; set; }
        public float WidthSizeChangeChance { get; set; }

        public float WidthPosShiftChance { get; set; }
        public int WidthPosShiftSize { get; set; }

        public int InitialNoPosShiftLength { get; set; }

        public float MaxWidthPosDrift { get; set; }

        public long Seed { get; set; }

        public int XRadius { get; set; }

        public int ZRadius { get; set; }


        public float LinePathNoiseScale { get; set; }

        public int MinOverlap { get; set; }

        public bool UseOvalWidth { get; set; }

        public int MinX { get; set; }
        public int MinZ { get; set; }

        public int MaxX { get; set; }
        public int MaxZ { get; set; }

        public LineGenParameters()
        {
            WidthSize = 1;
            MaxWidthSize = 1;
            MinWidthSize = 1;
            WidthSizeChangeAmount = 0;
            WidthSizeChangeChance = 0f;

            WidthPosShiftChance = 0f;
            WidthPosShiftSize = 0;
            InitialNoPosShiftLength = 0;
            LinePathNoiseScale = 0.0f;
            Seed = 0;
            MinOverlap = 1;

            MinX = -1000;
            MinZ = -1000;
            MaxX = 100000;
            MaxZ = 100000;

        }
    }
}


