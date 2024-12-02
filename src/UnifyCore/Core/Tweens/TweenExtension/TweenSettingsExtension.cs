namespace UnifyCore.Core.Tweens;

public static class TweenSettingsExtension
{
    public static T OnPlay<T>(this T t, TweenCallback callback) where T : Tween
    {
        if (t == null)
        {
            return null;
        }
        
        t.onPlay = callback;
        return t;
    }
    
    public static T OnUpdate<T>(this T t, TweenCallback callback) where T : Tween
    {
        if (t == null)
        {
            return null;
        }
        
        t.onUpdate = callback;
        return t;
    }
    
    public static T OnComplete<T>(this T t, TweenCallback callback) where T : Tween
    {
        if (t == null)
        {
            return null;
        }
        
        t.onComplete = callback;
        return t;
    }
}