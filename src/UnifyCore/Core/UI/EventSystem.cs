using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Unify2D.Core.Graphics;
using Microsoft.Xna.Framework;

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
        Vector2 mousePosition = Camera.Main.LocalToWorld(new Vector2 (mouseState.X, mouseState.Y));

        return mousePosition.X > obj.Position.X - obj.BoundingSize.X / 2f * obj.Scale.X
            && mousePosition.X < obj.Position.X + obj.BoundingSize.X / 2f * obj.Scale.X
            && mousePosition.Y > obj.Position.Y - obj.BoundingSize.Y / 2f * obj.Scale.Y
            && mousePosition.Y < obj.Position.Y + obj.BoundingSize.Y / 2f * obj.Scale.Y;
    }
}