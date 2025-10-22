using System;
using Microsoft.Xna.Framework;

namespace Unify2D.Core;

/// <summary>
/// This class inherit from <see cref="UIComponent"/> class and is used to create a button;
/// </summary>
public class UIButton : UIComponent, IPointerEventReceiver, IPointerHoverHandler
{
    public Color NormalColor { get; set; } = Color.White;
    public Color HoverColor { get; set; } = Color.White;
    public Color PressedColor { get; set; } = Color.White;
    public Color DisableColor { get; set; } = Color.Black;

    public bool Enabled { get; set; }
    public Action OnClick { get; set; }
    public Action OnPressed { get; set; }
    public Action OnRelease { get; set; }
    public Action OnHoverEvent { get; set; }
    public Action OnNotHoverEvent { get; set; }

    private UIImage _image;

    public override void Reset(GameObject go)
    {
        base.Reset(go);
        Console.WriteLine("reset : " + go.Name);

        _image = go.AddComponent<UIImage>();

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

    public override void Initialize(GameObject go)
    {
        base.Initialize(go);
        var goImage = go.GetComponent<UIImage>();
        if (_image == null && goImage != null)
        {
            _image = goImage;
        }
    }

    public override void Draw()
    {
    }

    public void OnPointerDown()
    {
        OnClick?.Invoke();
    }

    public void OnPointerPressed()
    {
        OnPressed?.Invoke();
        _image.Color = PressedColor;
    }

    public void OnPointerUp()
    {
        OnRelease?.Invoke();
        _image.Color = NormalColor;
    }

    public void OnHover()
    {
        OnHoverEvent?.Invoke();
        _image.Color = HoverColor;
    }

    public void OnNotHover()
    {
        OnNotHoverEvent?.Invoke();
        _image.Color = NormalColor;
    }
}