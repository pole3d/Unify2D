using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Unify2D.Core;

public class UIImage : UIComponent, IPointerEventReceiver
{
    public Texture2D Sprite { get; set; }
    public Color Color { get; set; } = Color.White;
    
    [JsonProperty]
    private GameAsset _asset;

    public override void Load(Game game, GameObject go)
    {
        _gameObject = go;
        try
        {
            _asset = new GameAsset(Sprite, _asset.Name);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    
    public override void Draw()
    {
        if (Sprite == null) return;
        
        Vector2 origin = Origin + (new Vector2(Sprite.Width, Sprite.Height)) * GetAnchorVector(Anchor);
        
        GameCore.Current.SpriteBatch.Draw(Sprite, _gameObject.Position,
            null, Color, _gameObject.Rotation, origin, _gameObject.Scale,
            SpriteEffects.None, 0);
    }

    public Action OnClick { get; set; }
    public Action OnPressed { get; set; }
    public Action OnRelease { get; set; }
    
    public void OnPointerClick()
    {
        OnClick?.Invoke();

        foreach (var component in GameObject.Components)
        {
            if (component is UIButton button)
            {
                button.OnButtonPressed?.Invoke();
            }
        }
    }
}