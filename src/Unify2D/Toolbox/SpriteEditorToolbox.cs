using ImGuiNET;

namespace Unify2D.Toolbox;

public class SpriteEditorToolbox : Toolbox
{
    private static bool _isOpen = false;
    
    public override void Draw()
    {
        if (!_isOpen)
        {
            return;
        }
        
        if (ImGui.Begin("Sprite Editor Toolbox", ref _isOpen))
        {
            
        }
        
        ImGui.End();
    }

    public static void Open()
    {
        _isOpen = true;
    }
}