using System.Collections.Generic;

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
            
            //TODO : detect IPointerEventReceiver elements

            _receivers.ForEach(r => r.OnPointerClick());
        }
        
        _receivers.ForEach(r => r.OnPointerPressed());
        
        if (mouseState.LeftButton == Input.ButtonState.Pressed)
        {
            _receivers.ForEach(r => r.OnPointerRelease());
            _receivers.Clear();
        }
    }
}