using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Toolbox;
using Unify2D.Toolbox.Popup;

namespace Unify2D
{
    internal class GameEditorUI
    {
        GameEditor _editor;

        Stack<PopupBase> _popups = new Stack<PopupBase>();


        public GameEditorUI(GameEditor editor)
        {
            _editor = editor;
        }

        public void DrawMainMenuBarUI()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Load project"))
                    {
                        _editor.ShowPopup(new LauncherPopup());
                    }
                    if (ImGui.MenuItem("Show Explorer"))
                    {
                        Process.Start("explorer.exe", _editor.Settings.Data.CurrentProjectPath);
                    }
                    if (ImGui.MenuItem("Build"))
                        _editor.Build();
                    if (ImGui.MenuItem("Save"))
                    {
                        _editor.SceneEditorManager.Save("test");
                    }
                    if (ImGui.MenuItem("Load"))
                    {
                        _editor.SceneEditorManager.LoadScene("test");
                    }
                    if (ImGui.MenuItem("Quit"))
                    {
                        _editor.Exit();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.MenuItem("Play"))
                {
                    _editor.Build();
                }

                ImGui.EndMainMenuBar();

                ImGui.ShowDemoWindow();

            }
        }

        public void DrawPopup()
        {
            if (_popups.Count > 0)
            {
                _popups.Peek().Draw(_editor);
            }
        }

        public void ShowPopup(PopupBase popup)
        {
            _popups.Push(popup);
        }

        internal void HidePopup()
        {
            _popups.Pop();
        }

    }
}
