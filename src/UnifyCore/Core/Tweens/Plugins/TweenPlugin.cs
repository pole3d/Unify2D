namespace UnifyCore.Core.Tweens;

public interface ITweenPlugin
{
}

public abstract class TweenPlugin<T1, T2, TOptions> : ITweenPlugin where TOptions : struct
{
    public abstract void SetFrom(Tweener<T1, T2, TOptions> tweener, bool isRelative);
    public abstract void Apply(TOptions options, Tween tween, bool isRelative, TweenGetter<T1> getter, TweenSetter<T1> setter, float elapsed, T2 startValue, T2 endValue, float duration);
}