using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        protected virtual void DrawInternal(GameEditor editor)
        {

        }
    }
}
