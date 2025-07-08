using ImGuiNET;

namespace Unify2D.Toolbox.Popup
{
    public abstract class PopupBase
    {
        public abstract string Name{  get;  }


        public void Draw(GameEditor editor)
        {
            if (ImGui.BeginPopupModal(Name))
            {
                DrawInternal(editor);

                ImGui.EndPopup();
            }

            ImGui.OpenPopup(Name);

        }

        protected abstract void DrawInternal(GameEditor editor);
    }
}
