using OxDb.Client.Assets.Constants;
using OxDb.SharedGame.Charms.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.UI.Charms
{

    public class CharmScreen : BaseScreen
    {
        const string CharmRowPrefabName = "CharmRow";
        public GameObject RowParent;


        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            PlayerCharmData charmData = _gs.ch.Get<PlayerCharmData>();

            foreach (PlayerCharm status in charmData.GetData())
            {
                _assetService.LoadAssetInto(RowParent, AssetCategoryNames.UI, CharmRowPrefabName, OnLoadStatusRow, token, status, "Charms");
            }

            await Task.CompletedTask;
        }

        private void OnLoadStatusRow(GameObject go, PlayerCharm charm, CancellationToken token)
        {
            CharmRow row = go.GetComponent<CharmRow>();
            if (row == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            row.Init(charm);
        }
    }
}


