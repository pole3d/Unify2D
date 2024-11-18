using System.Collections.Generic;

namespace Unify2D.Core;

/// <summary>
/// This class is a component used to hold different GameObject with <see cref="UIComponent"/> and manage them in a scene. 
/// </summary>
public class Canvas : Component
{
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public List<GameObject> Elements { get; set; }
}