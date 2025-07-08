using System;
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
