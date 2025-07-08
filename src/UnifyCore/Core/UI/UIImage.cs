using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Unify2D.Core;

public class UIImage : UIComponent, IPointerEventReceiver
{
    [JsonIgnore] public Texture2D Sprite { get; private set; }
    public string SpritePath { get; set; }

    public Color Color { get; set; } = Color.White;
    
    [JsonProperty]
    private GameAsset _asset;


    public void SetSprite(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        
        SpritePath = path;
        using FileStream filestream = new FileStream(path, FileMode.Open);
        Texture2D texture = Texture2D.FromStream(GameCore.Current.GraphicsDevice, filestream);
        Sprite = texture;
    }
    
    public override void Load(Game game, GameObject go)
    {
        base.Load(game,go);
        
        try
        {
            _asset = new GameAsset(Sprite, SpritePath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
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

        //TEST
        _gameObject.Scale *= 2;
        //---
    }
}