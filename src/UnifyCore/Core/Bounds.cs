
using Microsoft.Xna.Framework;
using System;

namespace UnifyCore.Core
{
    /// <summary>
    /// Defines the bounds a of gameObject, used when determining its position
    /// </summary>
    public class Bounds
    {
        public Action OnUpdate { get; set; }
        public Vector2 BoundingSize { get; set; }
        public Vector2 Pivot { get; set; }
        public Vector2 PositionOffset { get; set; }

        public Bounds(Vector2 boundingSize, Vector2 pivot, Vector2 positionOffset)
        {
            BoundingSize = boundingSize;
            Pivot = pivot;
            PositionOffset = positionOffset;
            OnUpdate?.Invoke();
        }

        public Bounds(Vector2 boundingSize)
        {
            BoundingSize = boundingSize;
            Pivot = .5f;
            PositionOffset = 0f;
            OnUpdate?.Invoke();
        }
    }
}
