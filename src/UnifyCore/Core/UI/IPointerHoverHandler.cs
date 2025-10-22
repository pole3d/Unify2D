using System;

namespace Unify2D.Core;

/// <summary>
/// Handles hovering the mouse above handles
/// </summary>
public interface IPointerHoverHandler
{
    public Action OnHoverEvent { get; set; }
    public Action OnNotHoverEvent { get; set; }

    /// <summary>
    /// Called the frame the mouse enters the handler
    /// </summary>
    public void OnHover()
    {
        OnHoverEvent?.Invoke();        
    }

    /// <summary>
    /// Called the frame the mouse exits the handler
    /// </summary>
    public void OnNotHover()
    {
        OnNotHoverEvent?.Invoke();
    }
}