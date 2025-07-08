namespace UnifyCore.Core.Tweens;

public class Tweener<T1, T2, TOptions> : Tween where TOptions : struct
{
    public TweenGetter<T1> Getter;
    public TweenSetter<T1> Setter;
    public T2 StartValue;
    public T2 EndValue;
    public TOptions TweenOption;
    public TweenPlugin<T1, T2, TOptions> TweenPlugin;

    public override bool Startup()
    {
        StartupDone = true;
        
        TweenPlugin.SetFrom(this, IsRelative);

        return true;
    }

    public override bool Update(float deltaTime)
    {
        if (!StartupDone)
        {
            if (!Startup())
            {
                return true;
            }
        }

        if (!PlayedOnce)
        {
            PlayedOnce = true;
            onPlay?.Invoke();
        }

        if (IsComplete)
        {
            return true;
        }

        onUpdate?.Invoke();

        Position += deltaTime;
        TweenPlugin.Apply(TweenOption, this, IsRelative, Getter, Setter, Position, StartValue, EndValue, Duration);
        return IsComplete;
    }

    public void Complete()
    {
        TweenPlugin.Apply(TweenOption, this, IsRelative, Getter, Setter, Duration, StartValue, EndValue, Duration);
        Position = Duration + 1.0f;
    }
}