namespace Application;

using System;

public sealed class Okay
{
    public void DiscardDesignation()
    {
        _ = "hello".Length;

        (int, int) NewPoint(int x, int y) => (x, y);
        var (one, _) = NewPoint(1, 2);

        void Out(out int x) => x = 3;
        Out(out _);
    }

    public void SwitchPatternMatching(object o)
    {
        switch (o)
        {
            case string _:
                break;
            case object _:
                break;
        }
    }

    public void IsPatternMatching(object o)
    {
        if (o is string _)
        {
        }
        else if (o is object _)
        {
        }
    }

    public void LambdaDiscardParameters/* C# 9 */()
    {
        Func<int, int, bool> alwaysFalse = (_, _) => false;
        Func<int, int, bool> alwaysTrue = (int _, int _) => true;
        var alwaysZero = delegate (int _, int _)
            {
                return 0;
            };
    }
}
