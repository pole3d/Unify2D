using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.IO;


namespace Unify2D.Core
{
    /// <summary>
    /// The <see cref="Component"/> class is an abstract base class that represents
    /// a component attached to a <see cref="GameObject"/>. Each <see cref="Component"/>
    /// holds a reference to its parent game object, and a <see cref="GameObject"/>, in turn,
    /// may have references to multiple <see cref="Component"/>s. This class serves as a foundation
    /// for creating specialized <see cref="Component"/>s with shared functionality.
    /// </summary>
    public abstract class Component
    {
        [JsonIgnore]
        public GameObject GameObject => _gameObject;

        protected GameObject _gameObject;

        /// <summary>
        /// Play only when the Component is created (via AddComponent)
        /// </summary>
        /// <param name="go"></param>
        public virtual void Reset(GameObject go) { }

        /// <summary>
        /// Play when the associated GameObject is created
        /// </summary>
        /// <param name="go"></param>
        public virtual void Initialize(GameObject go)
        {
            _gameObject = go;
        }

        public virtual void Load(Game game)
        {
        }

        public virtual void LateLoad(Game game)
        {

        }

        public virtual void Update(GameCore game)
        {

        }


        ///To be replaced by FixedUpdate later on
        public virtual void PhysicsUpdate(GameCore game)
        {

        }

        public virtual string GetImagePath(string filename)
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", filename);
            if (File.Exists(basePath)) return basePath;

            string secondaryPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", filename);
            if (File.Exists(secondaryPath)) return secondaryPath;

            string lastPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "Unify2D", "Images", filename);
            if (File.Exists(lastPath)) return lastPath;

            return basePath;
        }

        internal virtual void Destroy()
        {

        }

        internal virtual void DrawGizmo()
        {

        }
    }
}
