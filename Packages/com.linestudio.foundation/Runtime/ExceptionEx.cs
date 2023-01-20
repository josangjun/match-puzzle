using System;

public static partial class ExceptionEx
{
    public static Exception GetInnermostException(this Exception e)
    {
        var inner = e;
        while (inner.InnerException != null)
        {
            inner = inner.InnerException;
        }
        return inner;
    }
}
