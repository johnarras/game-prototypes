using Assets.Scripts.Crawler.Maps.Loading;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Riddles.Services;
using OxDb.SharedGame.Riddles.Settings;

namespace Assets.Scripts.Crawler.Maps.Props
{
    public class ToggleRiddleProp : CrawlerProp
    {

        protected IRiddleService _riddleService = null;
        private RiddleType _riddleType = null;

        protected int _index = 0;
        public override void SetData(CrawlerObjectLoadData loadData)
        {
            base.SetData(loadData);

            _riddleService.SetPropPosition(gameObject, loadData, GetToken());
            _riddleType = _gameData.Get<RiddleTypeSettings>(_gs.ch).Get(_mapRoot.Map.RiddleHints?.RiddleTypeId ?? 0);

            _index = _mapRoot.Map.GetEntityId(_cell.MapX, _cell.MapZ, EntityTypes.Riddle);

            UpdateToggle();
        }

        protected override void OnRedrawMapCellInternal(object obj)
        {
            UpdateToggle();
        }

        protected void UpdateToggle()
        {
            if (_riddleType == null || !_riddleType.IsToggle ||
                _party == null || _mapRoot == null)
            {
                return;
            }

            bool isOn = _party.HasRiddleBitIndex(_index);
            _clientEntityService.SetActive(OnObject, isOn);
            _clientEntityService.SetActive(OffObject, !isOn);
        }
    }
}


