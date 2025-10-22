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
        UpdatePointerHover(mouseState);
        UpdatePointerClick(mouseState);
    }

    private void UpdatePointerHover(MouseState mouseState)
    {
        IEnumerable<GameObject> objectsInScene = SceneManager.Instance.CurrentScene.GameObjectsWithChildren;
        foreach (GameObject obj in objectsInScene)
        {
            foreach (Component component in obj.Components)
            {
                if (component is IPointerHoverHandler receiver == false) continue;
                var isHovering = IsMouseInBounds(mouseState, obj);
                if (isHovering)
                    receiver.OnHover();
                else
                    receiver.OnNotHover();
            }
        }
    }

    private void UpdatePointerClick(MouseState mouseState)
    {
        if (mouseState.LeftButton == Input.ButtonState.Pressed)
        {
            _receivers.Clear();

            // TODO : Iterate over only UI GameObjects (by keeping track of them)
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

        var mousePosition = Camera.Main.LocalToWorld(new Vector2(mouseState.X, mouseState.Y));
        return obj.IsPointInBounds(mousePosition);
    }
}