using ImGuiNET;
using NativeFileDialogs.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            if ( ImGui.BeginMainMenuBar())
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
                        _editor.BuildProject();
                    if (ImGui.MenuItem("Save"))
                    {
                        // _editor.SceneEditorManager.SaveCurrentScene();
                        SaveCurrentScene();
                    }
                    if (ImGui.MenuItem("Load"))
                    {
                        //_editor.SceneEditorManager.LoadScene();
                        LoadScene();
                        Selection.UnSelectObject();
                    }
                    if (ImGui.MenuItem("Quit"))
                    {
                        _editor.Exit();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.MenuItem("Play"))
                {
                    _editor.BuildAndRun();
                }

                ImGui.EndMainMenuBar();

                ImGui.ShowDemoWindow();

            }
        }

        private void LoadScene()
        {
            string path = string.Empty;

            NfdStatus result = Nfd.OpenDialog(out path, new Dictionary<string, string>() { { "New Scene", "Scene" } }, Path.GetFullPath("./Assets"));

            if (result == NfdStatus.Ok)
            {
                SceneManager.Instance.LoadScene(path);
            }
        }

        private void SaveCurrentScene()
        {
            Scene scene = SceneManager.Instance.CurrentScene;

            if (scene.Name == null)
            {
                string path = string.Empty;
                NfdStatus result = Nfd.SaveDialog(out path, new Dictionary<string, string>() { { "New Scene", "scene" } }, "New Scene", Path.GetFullPath("./Assets").ToString());
                if (result == NfdStatus.Ok)
                {
                    scene.Path = path;
                    scene.Name = Path.GetFileName(path);
                }
            }

            if (scene.Name != null)
            {
                SceneManager.Instance.Save(scene);
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
