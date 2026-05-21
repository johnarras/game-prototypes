using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Riddles.Constants;
using OxDb.SharedGame.Riddles.Entities;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Riddles.EntranceRiddleHelpers
{
    public class MathRiddleTypeHelper : BaseRiddleTypeHelper
    {
        public override long HelperKey => RiddleTypes.Math;

        protected override async Task<bool> AddRiddleInternal(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            await Task.CompletedTask; if (lookup.ItemNames.Count < 1)
            {
                return false;
            }

            int itemQuantity = rand.Next(2, 3);

            List<string> itemNameDupe = new List<string>(lookup.ItemNames);

            List<string> wordNames = new List<string>();

            for (int i = 0; i < itemQuantity; i++)
            {
                string newWord = itemNameDupe[rand.Next() % itemNameDupe.Count];
                wordNames.Add(newWord);
                itemNameDupe.Remove(newWord);
            }

            StringBuilder riddleSb = new StringBuilder();

            riddleSb.Append("Alice has the following work order:\n\n");

            long total = 0;

            for (int i = 0; i < wordNames.Count; i++)
            {
                long purchaseQuantity = RandUtils.LongRange(3, 10, rand);
                long purchaseCost = RandUtils.LongRange(3, 10, rand);

                total += purchaseQuantity * purchaseCost;

                riddleSb.Append(purchaseQuantity + " " + wordNames[i] + " costing " + purchaseCost + " gold each.\n\n");
            }

            long totalGold = (100 * ((total / 100) + 1));

            riddleSb.Append("And they have " + totalGold + " gold currently.\n\n");

            riddleSb.Append("How much gold will they have after completing this purchase?\n\n");

            lockedFloor.AddFlags(CrawlerMapFlags.ShowFullRiddleText);
            lockedFloor.EntranceRiddle.Text = riddleSb.ToString();
            lockedFloor.EntranceRiddle.Answer = (totalGold - total).ToString();
            lockedFloor.EntranceRiddle.Error = "Sorry, that's not the correct amount!";

            return true;
        }
    }
}


