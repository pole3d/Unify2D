using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Toolbox.Popup;

namespace Unify2D.Toolbox
{
    internal class ValuePopup<T> : PopupBase
    {
        public override string Name => _name;
        
        private string _name;
        private List<T> _values;
        private Action<T> _onValueSelected;


        public ValuePopup(string name, List<T> values, Action<T> onValueSelected)
        {
            _name = name;
            _values = values;
            _onValueSelected = onValueSelected;
        }

        protected override void DrawInternal(GameEditor editor)
        {
            foreach(var value in _values)
            {
                if (ImGui.Button(value.ToString()))
                {
                    _onValueSelected?.Invoke(value);
                    editor.HidePopup();
                }
            }
        }


    }
}
