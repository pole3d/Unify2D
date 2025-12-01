using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;

namespace Unify2D.Core;

public class UIImage : UIComponent
{
    // Used by the GameAssetPropertyViewer to set the Texture - To Change
    public GameAsset AssetTexture { get; set; }

    [JsonIgnore]
    public Texture2D Texture { get; set; }
    public Color Color { get; set; } = Color.White;

    [JsonProperty] private string _imageGuid;

    public void SetAsset(GameAsset asset)
    {
        _imageGuid = asset.GUID;
        try
        {
            Texture = GameCore.Current.ResourcesManager.GetTexture(asset.Path);

            if (Texture != null)
            {
                UpdateBounds();
                OnAnchorUpdate += UpdateBounds;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }


    public override void Load(Game game)
    {
        base.Load(game);

        var asset = GameCore.Current.AssetsManager.GetAsset(_imageGuid);
        if (asset == null)
        {

            Debug.LogError($"Can't load image {_imageGuid} {_gameObject.Name}");
            return;
        }

        SetAsset(asset);
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

    private void UpdateBounds()
    {
        _gameObject.Bounds.BoundingSize = new Vector2(Texture.Width, Texture.Height);
        _gameObject.Bounds.PositionOffset = Origin;
        _gameObject.Bounds.Pivot = GetAnchorVector(Anchor);
    }

    internal override void Destroy()
    {
        OnAnchorUpdate -= UpdateBounds;
    }
}