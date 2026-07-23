
using OxDb.Client.Assets.Textures;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using System;
using UnityEngine;

namespace OxDb.Client.Crawler.UI.WorldUI
{
    public class DangerSenseUI : PartyBuffUI
    {

        public ColorLerp ColorLerp;

        protected override void FrameUpdateInternal(PartyData party)
        {

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            if (map == null)
            {
                return;
            }

            float sin = (float)Math.Round(MathF.Sin(-party.CurrPos.Rot * Mathf.PI / 180f));
            float cos = (float)Math.Round(Mathf.Cos(-party.CurrPos.Rot * Mathf.PI / 180f));

            float nx = cos * 1;
            float nz = sin * 1;

            int sx = party.CurrPos.X;
            int sz = party.CurrPos.Z;

            int ex = (int)(party.CurrPos.X + nx);
            int ez = (int)(party.CurrPos.Z + nz);

            int dx = ex - sx;
            int dz = ez - sz;

            int distance = 1;

            bool haveDanger = false;
            for (int d = 1; d <= distance; d++)
            {
                int cx = sx + dx * d;
                int cz = sz + dz * d;

                if (cx < 0 || cz < 0 || cx >= map.Width || cz >= map.Height)
                {
                    continue;
                }

                if (party.CurrentMap.Cleansed.HasBitIndex(map.GetIndex(cx, cz)))
                {
                    continue;
                }

                if (!party.CurrentMap.Visited.HasBitIndex(map.GetIndex(cx, cz)))
                {

                    int encounter = map.GetEntityId(cx, cz, EntityTypes.MapEncounter);

                    if (encounter > 0 && encounter != MapEncounters.Treasure && encounter != MapEncounters.Stats)
                    {
                        haveDanger = true;
                        break;
                    }
                }

                if (_crawlerMapService.GetMagicBits(party.CurrPos.MapId, cx, cz, true) > 0)
                {
                    haveDanger = true;
                    break;
                }
            }

            if (ColorLerp != null)
            {
                ColorLerp.SetLerpingNow(haveDanger);
            }
        }
    }
}


