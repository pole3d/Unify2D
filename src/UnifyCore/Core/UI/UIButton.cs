using System;
using Microsoft.Xna.Framework;

namespace Unify2D.Core;

/// <summary>
/// This class inherit from <see cref="UIComponent"/> class and is used to create a button;
/// </summary>
public class UIButton : UIComponent
{
    public Color NormalColor { get; set; } = Color.White;
    public Color HoverColor { get; set; } = Color.White;
    public Color PressedColor { get; set; } = Color.White;
    public Color DisableColor { get; set; } = Color.Black;

    public bool Enabled { get; set; }
    
    public Action OnButtonPressed { get; set; }
    
    private bool _isPressed;

    public override void Initialize(GameObject go)
    {
        base.Initialize(go);

        _gameObject.Name += "Button";
        
        go.AddComponent<UIImage>();
        
        GameObject textGameObject = GameObject.Create();
        textGameObject.Name = "Text";
        var text = textGameObject.AddComponent<UIText>();
        
        GameObject oldParent = textGameObject.Parent;
        GameObject.SetChild(_gameObject, textGameObject);
        if (oldParent != null && oldParent.Children.Contains(textGameObject))
        {
            oldParent.Children.Remove(textGameObject);
        }
        
        text.Text = "Button";
    }

    public override void Draw()
    {
    }
}