using ChipmunkSharp;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Physics
{
    internal class ChipmunkConverter
    {
        public static Vector2 CpVectToVector2(cpVect vect)
        {
            return new Vector2(vect.x, vect.y);
        }

        public static cpVect Vector2ToCpVect(Vector2 vect) 
        { 
            return new cpVect(vect.X, vect.Y);
        }
    }
}
