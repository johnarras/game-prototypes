

using OxDb.Client.PlayerSearch;
using OxDb.Client.UI.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.MapServer.WebApi.LoadIntoMap;
using OxDb.SharedPlatform.Accounts.PublicData;

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
                   _logService.Info("Acct: " + acct.Id + " -- " + acct.DisplayName);
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
                      _logService.Info("PUser: " + user.Id + " -- " + user.DisplayName);
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
                      _logService.Info("PChar: " + pchar.Id + " -- " + pchar.DisplayName);
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


