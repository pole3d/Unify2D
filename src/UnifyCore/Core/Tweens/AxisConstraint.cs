﻿using System;

namespace UnifyCore.Core.Tweens;

[Flags]
public enum AxisConstraint
{
    None = 0,
    X = 2,
    Y = 4,
    Z = 8,
    W = 16
}