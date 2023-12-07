using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Num = System.Numerics;

namespace Unify2D.Core
{
    public interface ICamera2D
    {
        public Color Background { get; set; }
        public float ZoomLevel { get; set; }
        public float Zoom { get; set; }
        public float Rotation { get; set; }
        public float RotationEuleur { get; set; }
        public Vector2 Viewport { get; set; }
        public Vector2 Position { get; set; }
        public Matrix Matrix { get; set; }
        public bool HasChanged { get; protected set; }

        public void UpdateMatrix()
        {
            HasChanged = false;

            Matrix =
            Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
            Matrix.CreateTranslation(new Vector3(Viewport.X * 0.5f, Viewport.Y * 0.5f, 0));
        }
        public Vector2 LocalToWorld(Vector2 local)
        {
            local -= Viewport * 0.5f;

            local /= Zoom; 


            // on divise par le zoom car Matrix.Up est calculé avec.
            Vector3 up = Matrix.Up / Zoom;
            float sin = -up.X;
            float cos = up.Y;

            Vector2 worldPosition = new Vector2(
                 (local.X * cos) + (local.Y * sin),
               -((local.X * sin) - (local.Y * cos)));

            worldPosition += Position;

            return worldPosition;
        }
        public Vector2 WorldToViewport(Vector2 world)
        {
            world -= Position;

            Vector3 up = Matrix.Up;
            float sin = -up.X;
            float cos = up.Y;

            Vector2 local = new Vector2(
                (world.X * cos) - (world.Y * sin),
                (world.X * sin) + (world.Y * cos));

            // on multiplie pas par le zoom car Matrix.Up en prends deja compte
            //local *= Zoom;

            local += Viewport * 0.5f;

            return local;
        }
    }
}
