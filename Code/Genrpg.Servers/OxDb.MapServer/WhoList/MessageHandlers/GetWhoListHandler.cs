using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.ServerCore.CloudComms.Servers.MapInstance.Queues;
using OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues;
using OxDb.ServerCore.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.WhoList.Entities;
using OxDb.SharedGame.WhoList.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.WhoList.MessageHandlers
{
    public class GetWhoListHandler : BaseCharacterServerMapMessageHandler<GetWhoList>
    {
        protected override async ValueTask InnerProcess(Character ch, GetWhoList message)
        {
            _cloudCommsService.SendResponseMessageWithHandler<WhoListResponse>(ServerNames.Player,
               new WhoListRequest() { Args = message.Args }, (response) => { OnReceiveWhoList(ch.Id, response); });
        }

        private void OnReceiveWhoList(string charId, WhoListResponse response)
        {
            if (!_objectManager.GetChar(charId, out Character ch))
            {
                return;
            }

            if (response == null)
            {
                ch.SendError("Bad Who List Response");
            }
            else
            {
                OnGetWhoList onGetList = new OnGetWhoList()
                {

                };

                foreach (WhoListChar wlc in response.Chars)
                {
                    onGetList.Items.Add(new WhoListItem()
                    {
                        Id = wlc.Id,
                        Name = wlc.Name,
                        Level = wlc.Level,
                        ZoneName = wlc.ZoneName,
                    });
                }
                ch.AddMessage(onGetList);
            }
        }
    }
}


