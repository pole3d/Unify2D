using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Toolbox.Popup;
using Num = System.Numerics;

namespace Unify2D.Toolbox
{
    internal class EnumPopup<T> : PopupBase where T : Enum
    {
        public override string Name => _name;
        
        private string _name;
        private T _values;
        private Action<T> _onValueSelected;


        public EnumPopup(string name, T values, Action<T> onValueSelected)
        {
            _name = name;
            _values = values;
            _onValueSelected = onValueSelected;
        }

        protected override void DrawInternal(GameEditor editor)
        {
            if (ImGui.Button("Close"))
            {
                editor.HidePopup();
            }

            foreach (T value in Enum.GetValues(typeof(T)))
            {
                bool selected = _values.HasFlag(value);

                if (selected) ImGui.PushStyleColor(ImGuiCol.Button, new Num.Vector4(0, 0.75f, 0, 1));

                if (ImGui.Button(value.ToString()))
                {
                    _onValueSelected?.Invoke(value);
                    editor.HidePopup();
                }

                if (selected) ImGui.PopStyleColor();
            }
        }


    }
}
