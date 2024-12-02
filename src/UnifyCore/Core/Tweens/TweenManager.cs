using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Unify2D.Core;
using UnifyCore.Core.Tweens.Options;
using UnifyCore.Core.Tweens.Plugins;

namespace UnifyCore.Core.Tweens;

public class TweenManager
{
    private static TweenManager s_Instance;
    public static TweenManager Instance => s_Instance ??= new TweenManager();

    private List<Tween> m_Tweens = new();
    
    public void Update(float deltaTime)
    {
        bool killTweens = false;
        foreach (Tween tween in m_Tweens)
        {
            if (tween.Update(deltaTime))
            {
                killTweens = true;
            }
        }

        if (killTweens)
        {
            KillTweens();
        }
    }

    private void KillTweens()
    {
        m_Tweens.RemoveAll(tween => tween.IsComplete);
    }

    private static Tweener<T1, T2, TOptions> To<T1, T2, TOptions>(TweenGetter<T1> getter, TweenSetter<T1> setter, T2 endValue, float duration, TweenPlugin<T1, T2, TOptions> tweenPlugin = null) where TOptions : struct
    {
        Tweener<T1, T2, TOptions> tweener = new Tweener<T1, T2, TOptions>
        {
            Getter = getter,
            Setter = setter,
            EndValue = endValue,
            Duration = duration,
            TweenPlugin = tweenPlugin ?? PluginManager.Get<T1, T2, TOptions>()
        };

        Instance.m_Tweens.Add(tweener);
        return tweener;
    }
    
    #region Move Tweens
    
    public static Tweener<Vector2, Vector2, VectorOptions> Move(GameObject gameObject, Vector2 endValue, float duration, bool snapping = false)
    {
        Tweener<Vector2, Vector2, VectorOptions> tweener = To<Vector2, Vector2, VectorOptions>(() => gameObject.Position, position => gameObject.Position = position, endValue, duration);
        tweener.Target = gameObject;
        tweener.TweenOption.Snapping = snapping;
        return tweener;
    }
    
    public static Tweener<Vector2, Vector2, VectorOptions> MoveX(GameObject gameObject, float endValue, float duration, bool snapping = false)
    {
        Tweener<Vector2, Vector2, VectorOptions> tweener = To<Vector2, Vector2, VectorOptions>(() => gameObject.Position, position => gameObject.Position = position, new Vector2(endValue, 0), duration);
        tweener.Target = gameObject;
        tweener.TweenOption.Constraint = AxisConstraint.X;
        tweener.TweenOption.Snapping = snapping;
        return tweener;
    }
    
    public static Tweener<Vector2, Vector2, VectorOptions> MoveY(GameObject gameObject, float endValue, float duration, bool snapping = false)
    {
        Tweener<Vector2, Vector2, VectorOptions> tweener = To<Vector2, Vector2, VectorOptions>(() => gameObject.Position, position => gameObject.Position = position, new Vector2(0, endValue), duration);
        tweener.Target = gameObject;
        tweener.TweenOption.Constraint = AxisConstraint.Y;
        tweener.TweenOption.Snapping = snapping;
        return tweener;
    }
    
    public static Tweener<Vector2, Vector2, VectorOptions> LocalMove(GameObject gameObject, Vector2 endValue, float duration, bool snapping = false)
    {
        Tweener<Vector2, Vector2, VectorOptions> tweener = To<Vector2, Vector2, VectorOptions>(() => gameObject.LocalPosition, position => gameObject.LocalPosition = position, endValue, duration);
        tweener.Target = gameObject;
        tweener.TweenOption.Snapping = snapping;
        return tweener;
    }
    
    public static Tweener<Vector2, Vector2, VectorOptions> LocalMoveX(GameObject gameObject, float endValue, float duration, bool snapping = false)
    {
        Tweener<Vector2, Vector2, VectorOptions> tweener = To<Vector2, Vector2, VectorOptions>(() => gameObject.LocalPosition, position => gameObject.LocalPosition = position, new Vector2(endValue, 0), duration);
        tweener.Target = gameObject;
        tweener.TweenOption.Constraint = AxisConstraint.X;
        tweener.TweenOption.Snapping = snapping;
        return tweener;
    }
    
    public static Tweener<Vector2, Vector2, VectorOptions> LocalMoveY(GameObject gameObject, float endValue, float duration, bool snapping = false)
    {
        Tweener<Vector2, Vector2, VectorOptions> tweener = To<Vector2, Vector2, VectorOptions>(() => gameObject.LocalPosition, position => gameObject.LocalPosition = position, new Vector2(0, endValue), duration);
        tweener.Target = gameObject;
        tweener.TweenOption.Constraint = AxisConstraint.Y;
        tweener.TweenOption.Snapping = snapping;
        return tweener;
    }
    
    #endregion
}