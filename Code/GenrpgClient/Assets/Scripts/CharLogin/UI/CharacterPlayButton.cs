
using Assets.Scripts.PlayerSearch;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.MapServer.WebApi.LoadIntoMap;

public class CharacterPlayButton : BaseBehaviour
{
    public GButton PlayButton;
    public GText CharText;

    protected IZoneGenService _zoneGenService = null;
    protected IPlayerSearchService _playerSearchService = null;
    protected IClientConfigContainer _config;

    private string _mapId;
    private string _charId;

    public void Init(string charId, string mapId, IScreen screen)
    {
        _mapId = mapId;
        _charId = charId;

        _uiService.SetButton(PlayButton, screen.GetName(), ClickPlay);
        _uiService.SetText(CharText, "Play " + _mapId);
    }

    public void ClickPlay()
    {
        _playerSearchService.AccountSearch(_gs.GameUserId,
           (PublicAccount acct) =>
           {
               if (acct != null)
               {
                   _logService.Info("Acct: " + acct.Id + " -- " + acct.Name);
               }
               else
               {
                   _logService.Info("Missing account");
               }
           },
           GetToken());

        _playerSearchService.UserSearch(_gs.GameUserId,
              (PublicUser user) =>
              {
                  if (user != null)
                  {
                      _logService.Info("PUser: " + user.Id + " -- " + user.Name);
                  }
                  else
                  {
                      _logService.Info("Missing PUser");
                  }
              },
            GetToken());


        _playerSearchService.CharacterSearch(_charId,
              (PublicCharacter pchar) =>
              {
                  if (pchar != null)
                  {
                      _logService.Info("PChar: " + pchar.Id + " -- " + pchar.Name);
                  }
                  else
                  {
                      _logService.Info("Missing PChar");
                  }
              },
            GetToken());


        LoadIntoMapRequest lwd = new LoadIntoMapRequest() { Env = _config.Config.Env, MapId = _mapId, CharId = _charId, GenerateMap = false };
        _zoneGenService.LoadMap(lwd);
    }
}


