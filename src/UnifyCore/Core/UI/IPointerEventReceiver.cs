using System;

namespace Unify2D.Core;

/// <summary>
/// Handles the click interactions with the mouse
/// </summary>
public interface IPointerEventReceiver
{
    public Action OnClick { get; set; }
    public Action OnPressed { get; set; }
    public Action OnRelease { get; set; }

    /// <summary>
    /// Called the frame the mouse is clicked
    /// </summary>
    public void OnPointerDown()
    {
        OnClick?.Invoke();        
    }

    /// <summary>
    /// Called continuously when the mouse is pressed
    /// </summary>
    public void OnPointerPressed()
    {
        OnPressed?.Invoke();
    }

    /// <summary>
    /// Called the frame the mouse is released
    /// </summary>
    public void OnPointerUp()
    {
        OnRelease?.Invoke();
    }
}