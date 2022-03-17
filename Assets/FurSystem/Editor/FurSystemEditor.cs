using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FurSystem))]
public class FurSystemEditor : Editor
{
    private bool wasMouseDown = false;

    private void OnSceneGUI()
    {
        FurSystem f = target as FurSystem;
        if (f.Edit)
        {
            Selection.activeGameObject = f.gameObject;
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            // if (wasMouseDown) return;
            // wasMouseDown = true;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if ((f.Copy != null && hit.collider.gameObject != f.Copy.gameObject) &&
                    (f.SkinnedCopy != null && hit.collider.gameObject != f.SkinnedCopy.gameObject)) return;
                if (Event.current.shift)
                {
                    f.AddTriangle(hit.triangleIndex);
                }
                else
                {
                    f.SetTriangle(hit.triangleIndex);
                }
                // do stuff
            }
        }
        // else if (Event.current.type == EventType.MouseDrag)
        // {
        //     Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        //
        //     RaycastHit hit;
        //     if (Physics.Raycast(ray, out hit))
        //     {
        //         if (Event.current.shift)
        //         {
        //             f.AddTriangle(hit.triangleIndex);
        //         }
        //
        //         // do stuff
        //     }
        // }
        else
        {
            wasMouseDown = false;
        }
    }

    public override void OnInspectorGUI()
    {
        FurSystem f = target as FurSystem;
        DrawDefaultInspector();
        if (GUILayout.Button("Save Current Patch"))
        {
            f.SavePatch();
        }

        if (GUILayout.Button("Draw patches"))
        {
            f.DrawPatches();
        }
    }
}