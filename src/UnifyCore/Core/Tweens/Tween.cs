namespace UnifyCore.Core.Tweens;

public abstract class Tween
{
    public TweenCallback onPlay;
    public TweenCallback onUpdate;
    public TweenCallback onComplete;

    public object Target;
    
    public float Duration;
    public float Position;
    public bool IsRelative;
    public bool StartupDone;
    public bool PlayedOnce;
    public bool IsComplete => Position >= Duration;

    public abstract bool Startup();
    
    public abstract bool Update(float deltaTime);
}