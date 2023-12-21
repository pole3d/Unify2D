using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Unify2D.Core;
using Unify2D.Physics;

namespace UnifyCore.Core.Physics
{
    public abstract class Collider : Component
    {
        [JsonIgnore]
        public Body Body { get { return _body; } }
        public Vector2 Offset { get { return _offset; } set { _offset = value; } }

        protected Vector2 _offset;

        [JsonIgnore]
        protected Body _body;
        protected float _gravityScale;
        protected bool _standalone = false;

        public abstract Body Init();
    }
}
