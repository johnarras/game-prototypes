using MessagePack;
namespace Genrpg.Shared.ProcGen.Settings.LineGen
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

        public int YRadius { get; set; }


        public float LinePathNoiseScale { get; set; }

        public int MinOverlap { get; set; }

        public bool UseOvalWidth { get; set; }

        public int XMin { get; set; }
        public int YMin { get; set; }

        public int XMax { get; set; }
        public int YMax { get; set; }

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

            XMin = -1000;
            YMin = -1000;
            XMax = 100000;
            YMax = 100000;

        }
    }
}


