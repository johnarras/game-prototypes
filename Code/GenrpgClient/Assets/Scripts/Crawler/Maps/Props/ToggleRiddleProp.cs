using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Riddles.Settings;

namespace Assets.Scripts.Crawler.Maps.Props
{
    public class ToggleRiddleProp : CrawlerProp
    {

        public RiddleType _riddleType = null;

        protected int _index = 0;
        public override void InitData(int x, int z, CrawlerMap map)
        {
            base.InitData(x, z, map);

            _riddleType = _gameData.Get<RiddleTypeSettings>(_gs.ch).Get(map.RiddleHints?.RiddleTypeId ?? 0);

            _index = map.GetEntityId(x, z, EntityTypes.Riddle);

            UpdateToggle();
        }

        protected override void OnRedrawMapCellInternal(object obj)
        {
            UpdateToggle();
        }

        protected void UpdateToggle()
        {
            if (_riddleType == null || !_riddleType.IsToggle ||
                _party == null || _map == null)
            {
                return;
            }

            bool isOn = _party.HasRiddleBitIndex(_index);
            _clientEntityService.SetActive(OnObject, isOn);
            _clientEntityService.SetActive(OffObject, !isOn);
        }
    }
}
