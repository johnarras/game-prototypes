using System.Text;

namespace Assets.Scripts.ProcGen.Materials
{
    public class CornerPoint
    {
        public int Index { get; set; }
        public int X { get; set; }
        public int Z { get; set; }

        public int OrigX { get; set; }

        public int OrigZ { get; set; }

        public int LeftIndex { get; set; }
        public int UpIndex { get; set; }
        public int RightIndex { get; set; }
        public int DownIndex { get; set; }


        public bool WasPerturbed { get; set; }

        public int ReplacesIndex { get; set; }

        public bool IsLeftReplace { get; set; }

        public bool IsUpReplace { get; set; }

        public CornerPoint(int x, int z)
        {
            OrigX = x;
            OrigZ = z;
            X = x;
            Z = z;
        }

        public string PrintData()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Corner: " + Index);
            sb.Append(" Orig: (" + OrigX + "," + OrigZ + ")");
            sb.Append(" Curr: (" + X + "," + Z + ")");
            sb.Append(" Up: " + UpIndex);
            sb.Append(" Down: " + DownIndex);
            sb.Append(" Left: " + LeftIndex);
            sb.Append(" Right: " + RightIndex);
            sb.Append(" WasPerturbed: " + WasPerturbed);
            sb.Append(" Replaces: " + ReplacesIndex);
            sb.Append(" SideReplace: " + (IsLeftReplace ? "Left" : "Right"));
            sb.Append(" UpDownReplace: " + (IsUpReplace ? "Up" : "Down"));

            return sb.ToString();
        }

    }
}
