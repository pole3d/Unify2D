using System;
using Microsoft.Xna.Framework;
using UnifyCore.Core.Utils;

namespace UnifyCore.Core.Tweens;

public class Vector2TweenPlugin : TweenPlugin<Vector2, Vector2, VectorOptions>
{
    public override void SetFrom(Tweener<Vector2, Vector2, VectorOptions> tweener, bool isRelative)
    {
        Vector2 previousEndValue = tweener.EndValue;
        tweener.StartValue = tweener.Getter();
        tweener.EndValue = isRelative ? tweener.StartValue + previousEndValue : previousEndValue;
        
        Vector2 to = tweener.StartValue;
        switch (tweener.TweenOption.Constraint)
        {
            case AxisConstraint.X:
            {
                to.X = tweener.EndValue.X;
                break;
            }
            
            case AxisConstraint.Y:
            {
                to.Y = tweener.EndValue.Y;
                break;
            }
            
            default:
            {
                to = tweener.EndValue;
                break;
            }
        }

        if (tweener.TweenOption.Snapping)
        {
            to.X = (float) Math.Round(to.X);
            to.Y = (float) Math.Round(to.Y);
        }

        tweener.Setter(to);
    }

    public override void Apply(VectorOptions options, Tween tween, bool isRelative, TweenGetter<Vector2> getter, TweenSetter<Vector2> setter, float elapsed, Vector2 startValue, Vector2 endValue, float duration)
    {
        float position = elapsed / duration;
        switch (options.Constraint)
        {
            case AxisConstraint.X:
            {
                Vector2 result = getter();
                result.X = MathUtils.Lerp(startValue.X, endValue.X, position);
                
                if (options.Snapping)
                {
                    result.X = (float) Math.Round(result.X);
                }

                setter(result);
                break;
            }
            
            case AxisConstraint.Y:
            {
                Vector2 result = getter();
                result.Y = MathUtils.Lerp(startValue.Y, endValue.Y, position);
                
                if (options.Snapping)
                {
                    result.Y = (float) Math.Round(result.Y);
                }

                setter(result);
                break;
            }

            default:
            {
                startValue.X = MathUtils.Lerp(startValue.X, endValue.X, position);
                startValue.Y = MathUtils.Lerp(startValue.Y, endValue.Y, position);

                if (options.Snapping)
                {
                    startValue.X = (float) Math.Round(startValue.X);
                    startValue.Y = (float) Math.Round(startValue.Y);
                }

                setter(startValue);
                break;
            }
        }
    }
}