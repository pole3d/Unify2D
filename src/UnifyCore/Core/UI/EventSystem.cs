using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Unify2D.Core.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;

namespace Unify2D.Core;

using Input = Microsoft.Xna.Framework.Input;

/// <summary>
/// This class is a component used to manage player's input and dispatch events to the UI.
/// </summary>
public class EventSystem : Component
{
    private List<IPointerEventReceiver> _receivers = new();
    private MouseState _lastMouseState;

    public override void Update(GameCore game)
    {
        var mouseState = Input.Mouse.GetState();
        if (mouseState.LeftButton == Input.ButtonState.Pressed)
        {
            _receivers.Clear();

            IEnumerable<GameObject> objectsInScene = SceneManager.Instance.CurrentScene.GameObjectsWithChildren;
            foreach (GameObject obj in objectsInScene)
            {
                if (IsMouseInBounds(mouseState, obj) == false) continue;

                foreach (Component component in obj.Components)
                {
                    if (component is IPointerEventReceiver receiver == false) continue;
                    _receivers.Add(receiver);
                }
            }


            if (_lastMouseState.LeftButton != mouseState.LeftButton) _receivers.ForEach(r => r.OnPointerDown());
            _receivers.ForEach(r => r.OnPointerPressed());
        }


        if (mouseState.LeftButton == Input.ButtonState.Released)
        {
            _receivers.ForEach(r => r.OnPointerUp());
            _receivers.Clear();
        }

        _lastMouseState = mouseState;
    }

    /// <summary>
    /// Check if the mouse is hovering a gameObject
    /// </summary>
    /// <param name="mouseState"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool IsMouseInBounds(MouseState mouseState, GameObject obj)
    {
        if (obj.HasUIComponents() == false) return false;

        var uiComponent = obj.UIComponents.First();
        var anchor = uiComponent.GetAnchorVector(uiComponent.Anchor) - new Vector2(0.5f);
        var origin = uiComponent.Origin;
        //Vector2 origin = Origin + (new Vector2(Sprite.Width, Sprite.Height)) * GetAnchorVector(Anchor);
        //Vector2 origin = uiComponent.Origin + obj.BoundingSize * GetAnchorVector(Anchor);



        Vector2 mousePosition = Camera.Main.LocalToWorld(new Vector2(mouseState.X, mouseState.Y));
        var sizeX = obj.BoundingSize.X * 0.5f * obj.Scale.X;
        var sizeY = obj.BoundingSize.Y * 0.5f * obj.Scale.Y;

        return mousePosition.X > obj.Position.X - origin.X - sizeX - (sizeX * 2f * anchor.X)
            && mousePosition.X < obj.Position.X - origin.X + sizeX - (sizeX * 2f * anchor.X)
            && mousePosition.Y > obj.Position.Y - origin.Y - sizeY - (sizeY * 2f * anchor.Y)
            && mousePosition.Y < obj.Position.Y - origin.Y + sizeY - (sizeY * 2f * anchor.Y);
    }
}