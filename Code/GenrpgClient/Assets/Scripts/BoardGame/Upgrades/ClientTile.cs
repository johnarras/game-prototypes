
using Assets.Scripts.Rewards.UI;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Tiles.Settings;
using Genrpg.Shared.UI.Constants;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.BoardGame.Tiles
{

    public class TileTypeWithIndex
    {
        public TileType TileType { get; set; }
        public int Index { get; set; }
        public int GridX { get; set; }
        public int GridZ { get; set; }
        public float XPos { get; set; }
        public float ZPos { get; set; }
    }

    public class ClientTile : BaseBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject PieceAnchor;
        public GAnimator Animator;
        public MeshFilter TileMesh;
        public MeshRenderer Renderer;
        public PopupRewardContainer RewardContainer;
        public GameObject[] PrizeAnchors;

        public short[] Prizes { get; set; } = new short[ExtraTileSlots.Max];

        private TileTypeWithIndex _tileType = null;

        public int GeTTileIndex()
        {
            return _tileType.Index; 
        }

        public TileTypeWithIndex GetTTI()
        {
            return _tileType;
        }

        public long GetTileTypeId()
        {
            return _tileType.TileType.IdKey;
        }

        public virtual void ClearPrizes(bool landing)
        {
            if (landing)
            {
                _clientEntityService.DestroyAllChildren(PrizeAnchors[ExtraTileSlots.Land]);
                Prizes[ExtraTileSlots.Land] = 0;
            }
            _clientEntityService.DestroyAllChildren(PrizeAnchors[ExtraTileSlots.Pass]);
            Prizes[ExtraTileSlots.Pass] = 0;
        }

        public void InitData(TileTypeWithIndex tileTypeWithIndex)
        {
            _tileType = tileTypeWithIndex;
        }

        public void ShowReward(IReward reward)
        {
            RewardContainer?.ShowReward(reward);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _screenService.Open(ScreenNames.Tile, _tileType);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ShowHighlight(false);
        }

        private void ShowHighlight(bool showNow)
        {
            transform.localScale = Vector3.one * (showNow ? 1.1f : 1.0f);
        }
    }
}
