using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;

namespace Unify2D.Assets
{
    internal abstract class AssetContent
    {
        public bool IsLoaded { get; set; }
        
        protected Asset _asset;

        public AssetContent(Asset asset)
        {
            _asset = asset;
        }
        
        public virtual void Load()
        {
            IsLoaded = true;
        }

        public virtual void Unload()
        {
            IsLoaded = false;
        }

        public virtual void OnDragDroppedInGame(GameEditor editor) { }
    }
}
