using System;
using System.Text;
using ImGuiNET;
using Unify2D.Core;
using Unify2D.Tools;

namespace Unify2D.Toolbox
{
    internal class HierarchyToolbox : Toolbox
    {

        bool[] _hierarchy = new bool[100];


        public override void Draw()
        {
            ImGui.Begin("Hierarchy");

            GameObject goToDestroy = null;

            ImGui.BeginChild("gameObjectList");

            int i = 0;
            foreach (var item in GameCore.Current.GameObjects)
            {
                ImGui.PushID(i++);
                if (ImGui.Selectable($"{item.Name}", _hierarchy[i]))
                {      

                    for (int j = 0; j < _hierarchy.Length; j++)
                    {
                        _hierarchy[j] = false;
                    }

                    _hierarchy[i] = true;
                    _editor.SelectObject(item);
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Button("Destroy"))
                    {
                        ImGui.CloseCurrentPopup();

                        goToDestroy = item;
                    }

                    ImGui.EndPopup();
                }
                
                if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
                {
                    unsafe
                    {
                        // Set payload to carry the index of our item (could be anything)
                        ImGui.SetDragDropPayload("HIERARCHY", (IntPtr)(&i), sizeof(int));
                    }
                    
                    Clipboard.DragContent = item;
                    ImGui.Text(item.Name);
                        
                    ImGui.EndDragDropSource();
                }
                ImGui.PopID();
            }

            ImGui.EndChild();
            
            // if (ImGui.BeginDragDropTarget())
            // {
            //     GameObject draggedGO = null;
            //     
            //     unsafe
            //     {
            //         var ptr = ImGui.AcceptDragDropPayload("HIERARCHY");
            //         if (ptr.NativePtr != null)
            //             draggedGO = Clipboard.DragContent as GameObject;
            //     }
            //
            //     if (draggedGO != null)
            //     {
            //         StringBuilder nameSb = new StringBuilder(draggedGO.Name);
            //         int safeguard = 0;
            //
            //         GameObject sameNameGO = null;
            //         foreach (GameObject go in GameCore.Current.GameObjects)
            //         {
            //             if (go.Name == draggedGO.)
            //         }
            //
            //         while (File.Exists(Path.Combine(_editor.AssetsPath, nameSb + ".prefab")))
            //         {
            //             if (++safeguard > 99999)
            //                 throw new Exception("Too many files with the name, or potentially stuck in an infinite loop. Prefab save failed.");
            //             
            //             char lastChar = nameSb[nameSb.Length - 1];
            //             if (char.IsDigit(lastChar))
            //             {
            //                 nameSb.Length--;
            //                 if (lastChar == '9')
            //                     nameSb.Append("10");
            //                 else
            //                     nameSb.Append((char)(lastChar + 1));
            //             }
            //             else
            //                 nameSb.Append('1');
            //         }
            //         nameSb.Append(".prefab");
            //         File.WriteAllText(Path.Combine(_editor.AssetsPath, nameSb.ToString()), JsonConvert.SerializeObject(draggedGO, new JsonSerializerSettings()));
            //         Reset();
            //     }
            //     ImGui.EndDragDropTarget();
            // }

            ImGui.End();

            if (goToDestroy != null)
            {
                GameCore.Current.DestroyImmediate(goToDestroy);
                _editor.UnSelectObject();
            }

        }
    }
}
