using Assets.Scripts.Core;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.Quests.WorldData;

public class QuestTaskUI : BaseBehaviour
{

    protected IMapProvider _mapProvider;
    protected IClientRandom _rand = null;

    public GText TaskText;

    private QuestType _qtype = null;
    private QuestTask _task = null;

    public void Init(QuestType qtype, QuestTask task)
    {
        if (qtype == null || task == null)
        {
            _clientEntityService.Destroy(entity);
            return;
        }

        _qtype = qtype;
        _task = task;

        ShowStatus();
    }

    public void ShowStatus()
    {
        if (_qtype == null || _task == null)
        {
            return;
        }

        _uiService.SetText(TaskText, _qtype.PrintTaskText(_rand.Rand, _gs.ch, _gameData, _mapProvider, _task.Index));

    }

}

