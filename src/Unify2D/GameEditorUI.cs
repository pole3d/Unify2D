using ImGuiNET;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NativeFileDialogs.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unify2D.Toolbox;
using Unify2D.Toolbox.Popup;
using UnifyCore;

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
                    ImGui.BeginDisabled(!_editor.IsProjectLoaded);
                    if (ImGui.MenuItem("Show Explorer"))
                    {
                        Process.Start("explorer.exe", _editor.Settings.Data.CurrentProjectPath);
                    }
                    if (ImGui.MenuItem("Build"))
                    {
                        Build();
                    }
                    if (ImGui.MenuItem("Create scene"))
                    {
                        // _editor.SceneEditorManager.SaveCurrentScene();
                        CreateNewScene();
                    }
                    if (ImGui.MenuItem("Save scene"))
                    {
                        // _editor.SceneEditorManager.SaveCurrentScene();
                        SaveCurrentScene();
                    }
                    if (ImGui.MenuItem("Load scene"))
                    {
                        //_editor.SceneEditorManager.LoadScene();
                        LoadScene();
                        Selection.UnSelectObject();
                    }
                    ImGui.EndDisabled();
                    if (ImGui.MenuItem("Quit"))
                    {
                        _editor.Exit();
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem(_editor.PlayToolbox?.IsPlaying == true ? "Stop" : "Play"))
                {
                    _editor.PlayToolbox?.TogglePlay();
                }

                ImGui.EndMainMenuBar();

            //    ImGui.ShowDemoWindow();
            }
        }

        public void Build()
        {
            _editor.PlayToolbox?.Stop();
            SaveCurrentScene();

            _editor.Build();
        }

        public void LoadScene()
        {
            string path = string.Empty;

            NfdStatus result = Nfd.OpenDialog(out path, new Dictionary<string, string>() { { "New Scene", "Scene" } }, Path.GetFullPath("./Assets"));

            if (result == NfdStatus.Ok)
            {
                _editor.PlayToolbox?.Stop();
                Selection.UnSelectObject();
                try { SceneManager.Instance.LoadSceneWithPath(path); }
                catch (Exception error) when (error is Newtonsoft.Json.JsonException || error is IOException)
                {
                    Debug.LogError(error.Message);
                }
            }
        }

        private void CreateNewScene(bool inAssetsToolBox = false)
        {
            if (inAssetsToolBox == false)
            {
                string path = string.Empty;
                NfdStatus result = Nfd.SaveDialog(out path, new Dictionary<string, string>() { { "New Scene", "scene" } }, "New Scene", Path.GetFullPath("./Assets").ToString());
                if (result == NfdStatus.Ok)
                {
                    _editor.PlayToolbox?.Stop();
                    Selection.UnSelectObject();
                    Scene scene = new Scene(Path.GetFileName(path),path);
                    try { SceneManager.Instance.LoadSceneWithPath(path); }
                    catch (Exception error) when (error is Newtonsoft.Json.JsonException || error is IOException)
                    {
                        Debug.LogError(error.Message);
                    }
                }
            }
            else
            {
                _editor.PlayToolbox?.Stop();
                string pathProject = Path.Combine(Directory.GetCurrentDirectory(), _editor.Settings.Data.CurrentProjectPath);
                SceneManager.Instance.CreateNewScene(pathProject, GameEditor.ScenesFolder);
            }
        }
        public void SaveCurrentScene()
        {
            // Play modifications are temporary; never serialize the running scene over the original.
            if (_editor.PlayToolbox?.IsPlaying == true) return;
            Scene scene = SceneManager.Instance.CurrentScene;

            if (scene.SceneInfo == null)
            {
                string path = string.Empty;
                NfdStatus result = Nfd.SaveDialog(out path, new Dictionary<string, string>() { { "New Scene", "scene" } }, "New Scene", Path.GetFullPath("./Assets").ToString());
                if (result == NfdStatus.Ok)
                {
                    scene.SetSceneInfo(Path.GetFileName(path), path);
                }
            }

            if (scene.SceneInfo != null)
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
