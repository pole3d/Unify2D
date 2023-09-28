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
    internal class FilePickerPopup : PopupBase
    {
        public override string Name => "FilePicker";
        public Action<string> OnPathSelected;

        protected override void DrawInternal(GameEditor editor)
        {
            var picker = FilePicker.GetFolderPicker(this, editor.ProjectPath);
            picker.RootFolder = "C:\\";
            picker.OnlyAllowFolders = true;
            if (picker.Draw())
            {
                OnPathSelected?.Invoke(picker.SelectedFile);

                FilePicker.RemoveFilePicker(this);
                editor.HidePopup();

            }
        }


    }
}
