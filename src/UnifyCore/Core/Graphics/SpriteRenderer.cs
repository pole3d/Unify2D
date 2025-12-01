using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;


namespace Unify2D.Core.Graphics
{
    /// <summary>
    /// The <see cref="SpriteRenderer"/> class extends the functionality of
    /// the base <see cref="Renderer"/> class and introduces properties for color
    /// and a sprite <see cref="GameAsset"/>. This class is specifically designed
    /// for rendering 2D sprites in the game world.
    /// </summary>
    public class SpriteRenderer : Renderer
    {
        // Used by the GameAssetPropertyViewer to set the Texture - To Change
        public GameAsset AssetTexture{ get; set; }

        [JsonIgnore]
        public Texture2D Texture { get; set; }

        public Color Color { get; set; } = Color.White;
        public float LayerDepth { get; set; } = 0f;


        public override void Initialize(GameObject go)
        {
            base.Initialize(go);

            Texture = GameCore.Current.ResourcesManager.GetTexture(null);
            _gameObject.Bounds.BoundingSize = new Vector2(Texture.Width, Texture.Height);
            _gameObject.Bounds.Pivot = 0.5f;
        }

        public void Initialize(Game game, GameObject go, GameAsset asset)
        {
            
            _gameObject = go;
            AssetTexture = asset;

            try
            {
                if (AssetTexture != null)
                {
                    Texture = GameCore.Current.ResourcesManager.GetTexture(asset.Path);
       
                    _gameObject.Bounds.BoundingSize = new Vector2(Texture.Width, Texture.Height);
                    _gameObject.Bounds.Pivot = 0.5f;
                }

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        internal override void Destroy()
        {
            Texture = null;
            if (AssetTexture != null)
                AssetTexture.Release();
        }


        public override void Load(Game game, GameObject go)
        {
            if ( AssetTexture == null )
            {
                return;
            }

            var asset = GameCore.Current.AssetsManager.GetAsset(AssetTexture.GUID);
            if (asset == null)
            {
                Debug.LogError($"Can't load sprite {AssetTexture.GUID} {_gameObject.Name}");
                return;
            }

            Initialize(game, go, asset);
        }

        public override void Draw()
        {
            if (Texture == null)
                return;

            GameCore.Current.SpriteBatch.Draw(Texture, _gameObject.Position,
     null, Color, _gameObject.Rotation, _gameObject.Bounds.BoundingSize / 2, _gameObject.Scale,
     SpriteEffects.None, LayerDepth);
        }
    }
}
