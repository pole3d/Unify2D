using System;
using Microsoft.Xna.Framework;

namespace UnifyCore.Core.Tweens.Plugins;

public class PluginManager
{
    private static ITweenPlugin s_Vector2Plugin;
    
    public static TweenPlugin<T1, T2, TOptions> Get<T1, T2, TOptions>() where TOptions : struct
    {
        Type t1 = typeof(T1);
        Type t2 = typeof(T2);

        ITweenPlugin plugin = null;
        if (t1 == typeof(Vector2) && t1 == t2)
        {
            plugin = s_Vector2Plugin ??= new Vector2TweenPlugin();
        }

        return plugin as TweenPlugin<T1, T2, TOptions>;
    }
}