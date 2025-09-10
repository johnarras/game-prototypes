
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using System;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class DangerSenseUI : AnimatedPartyBuffUI
    {
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

            int sx = party.CurrPos.Rot;
            int sz = party.CurrPos.Rot;

            int ex = (int)(party.CurrPos.Rot + nx);
            int ez = (int)(party.CurrPos.Rot + nz);

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
                    if (!map.HasFlag(CrawlerMapFlags.IsLooping))
                    {
                        return;
                    }
                    cx = (cx + map.Width) % map.Width;
                    cz = (cz + map.Height) % map.Height;
                }

                if (party.CurrentMap.Cleansed.HasBit(map.GetIndex(cx, cz)))
                {
                    continue;
                }

                if (!party.CurrentMap.Visited.HasBit(map.GetIndex(cx, cz)))
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

            Sprite.OnlyShowFirstFrame = !haveDanger;
        }
    }
}
