
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;

namespace Unify2D.Core
{
    /// <summary>
    /// The <see cref="UIComponent"/> class, an abstract extension of <see cref="Component"/>,
    /// it serves as a base class for UI elements in the game view.
    /// </summary>
    public abstract class UIComponent : Component
    {
        [JsonIgnore]
        public GameObject ParentCanvas { get; set; }

        public Vector2 Origin { get; set; }

        public enum AnchorType
        {
            UpperLeft = 0,
            UpperCenter = 1,
            UpperRight = 2,
            MiddleLeft = 3,
            MiddleCenter = 4,
            MiddleRight = 5,
            LowerLeft = 6,
            LowerCenter = 7,
            LowerRight = 8,
        }

        public AnchorType Anchor
        {
            get => _anchor;
            set
            {
                _anchor = value;
                OnAnchorUpdate?.Invoke();
            }
        }
        [JsonIgnore] public Action OnAnchorUpdate { get; set; }

        private AnchorType _anchor;

        public Vector2 GetAnchorVector(AnchorType anchor)
        {
            switch (anchor)
            {
                case AnchorType.UpperLeft:
                    return new Vector2(0, 0);
                case AnchorType.UpperCenter:
                    return new Vector2(0.5f, 0);
                case AnchorType.UpperRight:
                    return new Vector2(1, 0);
                case AnchorType.MiddleLeft:
                    return new Vector2(0, 0.5f);
                case AnchorType.MiddleCenter:
                    return new Vector2(0.5f, 0.5f);
                case AnchorType.MiddleRight:
                    return new Vector2(1, 0.5f);
                case AnchorType.LowerLeft:
                    return new Vector2(0, 1);
                case AnchorType.LowerCenter:
                    return new Vector2(0.5f, 1);
                case AnchorType.LowerRight:
                    return new Vector2(1, 1);
            }
            return Vector2.Zero;
        }
        
        public abstract void Draw();
    }
}