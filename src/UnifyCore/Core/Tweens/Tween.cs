using System;

namespace UnifyCore.Core.Tweens;

public abstract class Tween
{
    public Action onPlay;
    public Action onUpdate;
    public Action onComplete;

    public object Target;
    
    public float Duration;
    public float Position;
    public bool IsRelative;
    public bool StartupDone;
    public bool IsComplete => Position >= Duration;

    public abstract bool Startup();
    
    public abstract bool Update(float deltaTime);
}