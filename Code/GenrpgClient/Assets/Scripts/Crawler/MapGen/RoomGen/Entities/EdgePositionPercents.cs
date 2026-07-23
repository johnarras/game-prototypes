namespace OxDb.Client.Crawler.MapGen.RoomGen.Entities
{
    public class EdgePositionPercents
    {
        public float PercentFromMid { get; set; }
        public float PercentFromLeft { get; set; }
        public float PercentFromRight { get; set; }
        public float FinalPercent { get; set; }

        public EdgePositionPercents(float percentFromMid, float percentFromLeft, float percentFromRight, float finalPercent)
        {
            PercentFromMid = percentFromMid;
            PercentFromLeft = percentFromLeft;
            PercentFromRight = percentFromRight;
            FinalPercent = finalPercent;
        }
    }
}
