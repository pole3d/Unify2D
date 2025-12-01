using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;

namespace Unify2D.Core;

public class UIImage : UIComponent
{
    [JsonIgnore]
    public Texture2D Texture { get; set; }
    public Color Color { get; set; } = Color.White;

    [JsonProperty]
    string _imageGuid
    { get => guidField; 
      set => guidField = value;
    }

    string guidField;
    GameAsset _asset;

    public void Initialize(Game game, GameObject go, GameAsset asset)
    {
        _gameObject = go;
        _asset = asset;
        _imageGuid = asset.GUID;


        try
        {
            Texture = asset.LoadTexture();

            if (Texture != null)
            {
                _gameObject.Bounds.BoundingSize = new Vector2(Texture.Width, Texture.Height);
                _gameObject.Bounds.PositionOffset = Origin;
                _gameObject.Bounds.Pivot = GetAnchorVector(Anchor);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public override void Load(Game game, GameObject go)
    {
        base.Load(game, go);

        var asset = GameCore.Current.AssetsManager.GetAsset(_imageGuid);
        if (asset == null)
        {

            Debug.LogError($"Can't load image {_imageGuid} {_gameObject.Name}");
            return;
        }

        Initialize(game, go, asset);
    }

    public override void Draw()
    {
        if (_gameObject == null)
        {
            return;
        }

        if (Texture == null)
        {
            //SetSprite(_game, _gameObject, _asset);
            return;
        }

        Vector2 origin = Origin + (new Vector2(Texture.Width, Texture.Height)) * GetAnchorVector(Anchor);

        GameCore.Current.SpriteBatch.Draw(Texture, _gameObject.Position,
            null, Color, _gameObject.Rotation, origin, _gameObject.Scale,
            SpriteEffects.None, 0);
    }
}