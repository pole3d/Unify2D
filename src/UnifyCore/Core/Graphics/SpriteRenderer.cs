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

        [JsonProperty]
        string _spriteGuid;

        Texture2D _texture;
        GameAsset _asset;

        public void Initialize(Game game, GameObject go, GameAsset asset)
        {
            _gameObject = go;
            _asset = asset;
            _spriteGuid = asset.GUID;
            try
            {

                _texture = asset.LoadTexture();

                if (_texture != null)
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
            var asset = GameCore.Current.AssetsManager.GetAsset(_spriteGuid);
            if ( asset == null)
            {
                Debug.LogError($"Can't load sprite {_spriteGuid} {_gameObject.Name}");
                return;
            }

            Initialize(game, go, asset);
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
