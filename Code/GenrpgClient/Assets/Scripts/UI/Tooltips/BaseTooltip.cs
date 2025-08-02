using System.Threading;

public class InitTooltipData
{
}


public abstract class BaseTooltip : BaseBehaviour
{
    protected CancellationToken _token;
    public virtual void Init(InitTooltipData initData, CancellationToken token)
    {
        _token = token;
    }
}