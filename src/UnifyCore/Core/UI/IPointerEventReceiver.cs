using System;

namespace Unify2D.Core;

public interface IPointerEventReceiver
{
    public Action OnClick { get; set; }
    public Action OnPressed { get; set; }
    public Action OnRelease { get; set; }

    public void OnPointerClick()
    {
        OnClick?.Invoke();        
    }
    
    public void OnPointerPressed()
    {
        OnPressed?.Invoke();
    }
    
    public void OnPointerRelease()
    {
        OnRelease?.Invoke();
    }
}