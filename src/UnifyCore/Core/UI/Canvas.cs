using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Unify2D.Core;

/// <summary>
/// This class is a component used to hold different GameObject with <see cref="UIComponent"/> and manage them in a scene. 
/// </summary>
public class Canvas : Component
{
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;

    [JsonIgnore] public List<UIComponent> Elements { get; set; } = new();

    public void UpdateList()
    {
        Debug.Log("update list");

        Elements.Clear();

        if (_gameObject == null || _gameObject.Children == null) return;
        foreach (GameObject child in _gameObject.Children)
        {
            SetCanvasParentForChildren(child, this);
        }
    }

    private static void SetCanvasParentForChildren(GameObject parent, Canvas canvas)
    {
        foreach (Component component in parent.Components)
        {
            if (component is not UIComponent ui) continue;
            ui.ParentCanvas = canvas.GameObject;
            canvas.Elements.Add(ui);
        }

        if (parent.Children == null) return;
        foreach (GameObject child in parent.Children)
        {
            SetCanvasParentForChildren(child, canvas);
        }
    }

    public void Draw()
    {
        foreach (UIComponent component in Elements)
        {
            component.Draw();
        }
    }
}