using System.Threading;


public class TokenUtils
{
    public static bool IsValid(CancellationToken token)
    {
        return token != CancellationToken.None && !token.IsCancellationRequested;
    }
}


