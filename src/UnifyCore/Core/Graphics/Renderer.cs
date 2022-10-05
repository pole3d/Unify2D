using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Core.Graphics
{

    public abstract class Renderer : Component
    {

        public override void Load(Game game, GameObject go)
        {

        }

        public abstract void Draw();
    }
}
