using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Unify2D.Core;

public class UIImage : UIComponent
{
    [JsonIgnore] public Texture2D Sprite { get; private set; }
    public string SpritePath { get; set; }

    public Color Color { get; set; } = Color.White;
    
    //[JsonProperty]
    //private GameAsset _asset;


    public void SetSprite(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        
        SpritePath = path;

        Sprite = GameCore.Current.ResourcesManager.GetTexture(path);

        if (Sprite != null)
        {
            _gameObject.Bounds.BoundingSize = new Vector2(Sprite.Width, Sprite.Height);
            _gameObject.Bounds.PositionOffset = Origin;
            _gameObject.Bounds.Pivot = GetAnchorVector(Anchor);
        }
    }
    
    public override void Load(Game game, GameObject go)
    {
        base.Load(game,go);
        
        //try
        //{
        //    _asset = new GameAsset(Sprite, SpritePath);
        //}
        //catch (Exception e)
        //{
        //    Console.WriteLine(e.ToString());
        //}
        SetSprite(SpritePath);
    }
    
    public override void Draw()
    {
        if (_gameObject == null)
        {
            return;
        }
        
        if (Sprite == null)
        {
            SetSprite(SpritePath);
            return;
        }
        
        Vector2 origin = Origin + (new Vector2(Sprite.Width, Sprite.Height)) * GetAnchorVector(Anchor);
        
        GameCore.Current.SpriteBatch.Draw(Sprite, _gameObject.Position,
            null, Color, _gameObject.Rotation, origin, _gameObject.Scale,
            SpriteEffects.None, 0);
    }
}