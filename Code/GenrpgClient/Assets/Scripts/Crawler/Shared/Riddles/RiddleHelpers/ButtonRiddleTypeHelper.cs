using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.UI.Dungeons;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Riddles.Constants;
using Genrpg.Shared.Riddles.Entities;
using Genrpg.Shared.Riddles.EntranceRiddleHelpers;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Shared.Riddles.RiddleHelpers
{
    public class ButtonRiddleTypeHelper : BaseRiddleTypeHelper
    {
        private ICrawlerWorldService _worldService = null;
        private ICrawlerMapService _mapService = null;
        public override long Key => RiddleTypes.Buttons;

        public override void SetPropPosition(object prop, object data, CancellationToken token)
        {
            GameObject go = prop as GameObject;
            if (go == null)
            {
                return;
            }

            WallButton wb = go.GetComponent<WallButton>();

            if (wb == null || wb.MeshRenderer == null)
            {
                return;
            }

            CrawlerObjectLoadData loadData = data as CrawlerObjectLoadData;

            if (loadData == null)
            {
                return;
            }

            IRandom rand = new MyRandom(loadData.Seed);

            float edgeDelta = 2;
            float xpos = MathUtils.FloatRange(-CrawlerMapConstants.XZBlockSize + edgeDelta, CrawlerMapConstants.XZBlockSize - edgeDelta, rand) / 2;
            float ypos = MathUtils.FloatRange(-CrawlerMapConstants.YBlockSize + edgeDelta, CrawlerMapConstants.YBlockSize - edgeDelta, rand) / 2;

            Vector3 pos = wb.MeshRenderer.gameObject.transform.localPosition;

            Vector3 newPos = pos + new Vector3(xpos, ypos, 0);
            wb.MeshRenderer.gameObject.transform.localPosition = newPos;
        }

        protected override async Task<bool> AddRiddleInternal(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            int buttonCount = rand.Next(3, 5);

            int buttonsPlaced = 0;
            int riddleAnswer = 0;
            while (buttonsPlaced < buttonCount && openPoints.Count > 0)
            {
                PointXZ openPoint = openPoints[rand.Next(openPoints.Count)];
                openPoints.Remove(openPoint);

                MapDir[] dirs = MapDirs.GetDirs();

                bool[] validWall = new bool[dirs.Length];

                for (int d = 0; d < dirs.Length; d++)
                {
                    validWall[d] = _mapService.GetBlockingBits(prevFloor, openPoint.X, openPoint.Z, openPoint.X + dirs[d].DX,
                        openPoint.Z + dirs[d].DZ, false) == WallTypes.Wall;
                }

                int numChoices = validWall.Where(x => x == true).Count();
                if (numChoices < 1)
                {
                    continue;
                }
                int choiceChosen = rand.Next(numChoices);

                int wallIndex = -1;
                for (int i = 0; i < validWall.Length; i++)
                {
                    if (validWall[i])
                    {
                        choiceChosen--;
                    }
                    if (choiceChosen < 0)
                    {
                        wallIndex = i;
                        break;
                    }
                }
                if (wallIndex >= 0)
                {
                    prevFloor.SetEntity(openPoint.X, openPoint.Z, EntityTypes.Riddle, buttonsPlaced + 1);
                    prevFloor.Set(openPoint.X, openPoint.Z, CellIndex.Dir, wallIndex);
                    riddleAnswer |= (1 << buttonsPlaced);
                    buttonsPlaced++;
                }
            }

            if (buttonsPlaced != buttonCount)
            {
                return false;
            }

            lockedFloor.AddFlags(CrawlerMapFlags.ShowFullRiddleText);
            lockedFloor.EntranceRiddle.Text = "There are " + buttonCount + " bars blocking the trapdoor to the next floor.";
            lockedFloor.EntranceRiddle.Answer = riddleAnswer.ToString();
            lockedFloor.EntranceRiddle.Error = "Sorry, the orbs are not correctly set...";
            await Task.CompletedTask;
            return true;
        }

        public override bool ShouldDrawProp(PartyData party, int x, int z)
        {
            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            int index = map.GetEntityId(x, z, EntityTypes.Riddle);

            if (index > 0)
            {
                return !party.HasRiddleBitIndex((index - 1));
            }
            return true;
        }
    }
}
