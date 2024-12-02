using System.Collections.Generic;
using UnifyCore;

namespace Unify2D.Core;

using Input = Microsoft.Xna.Framework.Input;

/// <summary>
/// This class is a component used to manage player's input and dispatch events to the UI.
/// </summary>
public class EventSystem : Component
{
    private List<IPointerEventReceiver> _receivers = new();

    public override void Update(GameCore game)
    {
        var mouseState = Input.Mouse.GetState();
        if (mouseState.LeftButton == Input.ButtonState.Pressed)
        {
            _receivers.Clear();

            List<GameObject> objectsInScene = SceneManager.Instance.CurrentScene.GameObjects;
            foreach (GameObject obj in objectsInScene)
            {
                //check if mouse in bounds
                
                foreach (Component component in obj.Components)
                {
                    if (component is IPointerEventReceiver receiver == false) continue;
                    _receivers.Add(receiver);
                }
            }

            _receivers.ForEach(r => r.OnPointerClick());
        }

        _receivers.ForEach(r => r.OnPointerPressed());

        if (mouseState.LeftButton == Input.ButtonState.Released)
        {
            _receivers.ForEach(r => r.OnPointerRelease());
            _receivers.Clear();
        }
    }
}