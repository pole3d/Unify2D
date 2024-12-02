namespace UnifyCore.Core.Tweens;

public delegate T TweenGetter<out T>();

public delegate void TweenSetter<in T>(T newValue);