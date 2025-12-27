
using System.Threading;
using UnityEngine;

public class AddQuests : BaseZoneGenerator
{

    protected IQuestGenService _questGenService = null;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        _questGenService.GenerateQuests(_gs);
    }

}

