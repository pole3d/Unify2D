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
        public Color Color { get; set; } = Color.White;
        public float LayerDepth { get; set; } = 0f;
        [JsonIgnore]
        public GameAsset Asset => _asset;

        [JsonProperty]
        GameAsset _asset;
        Texture2D _texture;

        public void Initialize(Game game, GameObject go, string path)
        {
            _gameObject = go;
            try
            {
                _texture = GameCore.Current.ResourcesManager.GetTexture(path);

                _asset = new GameAsset(_texture, path);
                _gameObject.BoundingSize = new Vector2(_texture.Width, _texture.Height);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        internal override void Destroy()
        {
            _texture = null;
            if (_asset != null)
                _asset.Release();
        }

        public override void Load(Game game, GameObject go)
        {
            if (_asset != null)
                Initialize(game, go, _asset.Name);
        }

        public override void Draw()
        {
            if (_texture == null)
                return;

            GameCore.Current.SpriteBatch.Draw(_texture, _gameObject.Position,
     null, Color, _gameObject.Rotation, _gameObject.BoundingSize / 2, _gameObject.Scale,
     SpriteEffects.None, LayerDepth);
        }
    }
}
