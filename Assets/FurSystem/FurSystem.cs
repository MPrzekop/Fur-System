using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


[RequireComponent(typeof(Renderer))]
public class FurSystem : MonoBehaviour
{
    [SerializeField] private bool edit;
    [SerializeField] private MeshFilter original, copy;


    [SerializeField] private SkinnedMeshRenderer skinnedOriginal, skinnedCopy;

    [SerializeField] private Mesh selectingVis;
    [SerializeField] private bool skinned;
    [SerializeField] private List<Mesh> currentTriangle;

    [SerializeField] private List<FurPatchObject> patches;
    [SerializeField] private FurPatchData currentPatchData;


    public MeshFilter Copy
    {
        get => copy;
        set => copy = value;
    }

    public MeshFilter Original
    {
        get => original;
        set => original = value;
    }

    public bool Edit
    {
        get => edit;
        set => edit = value;
    }

    public SkinnedMeshRenderer SkinnedCopy
    {
        get => skinnedCopy;
        set => skinnedCopy = value;
    }


    private void OnValidate()
    {
        if (GetComponent<SkinnedMeshRenderer>() != null)
        {
            SetupSkinned();
            return;
        }
        else
        {
            SetupBasic();
        }
    }

    void SetupSkinned()
    {
        skinned = true;
        skinnedOriginal = GetComponent<SkinnedMeshRenderer>();
        if (SkinnedCopy == null)
        {
            if (skinnedOriginal == null) return;
            var newGO = new GameObject("edit holder");
            newGO.transform.SetParent(skinnedOriginal.transform);
            newGO.transform.localPosition = Vector3.zero;
            newGO.transform.localScale = Vector3.one;
            newGO.transform.localRotation = Quaternion.identity;
            SkinnedCopy = newGO.AddComponent<SkinnedMeshRenderer>();
            SkinnedCopy.sharedMesh = Instantiate(skinnedOriginal.sharedMesh);
            selectingVis = new Mesh();
            skinnedOriginal.BakeMesh(selectingVis);
            SkinnedCopy.sharedMesh.boneWeights = skinnedOriginal.sharedMesh.boneWeights;
            SkinnedCopy.sharedMesh.bindposes = skinnedOriginal.sharedMesh.bindposes;
            SkinnedCopy.bones = skinnedOriginal.bones;
            SkinnedCopy.rootBone = skinnedOriginal.rootBone;
            newGO.AddComponent<MeshCollider>().sharedMesh = new Mesh();
            SkinnedCopy.BakeMesh(newGO.GetComponent<MeshCollider>().sharedMesh);
        }

        if (currentTriangle == null)
        {
            currentTriangle = new List<Mesh>();
        }

        patches.RemoveAll(x => x == null);
    }

    void SetupBasic()
    {
        if (Copy == null)
        {
            if (Original == null) return;
            var newGO = new GameObject("edit holder");
            newGO.transform.SetParent(Original.transform);
            newGO.transform.localPosition = Vector3.zero;
            newGO.transform.localScale = Vector3.one;
            newGO.transform.localRotation = Quaternion.identity;
            Copy = newGO.AddComponent<MeshFilter>();
            Copy.mesh = Original.mesh;
            selectingVis = Copy.mesh;
            newGO.AddComponent<MeshCollider>();
        }

        if (currentTriangle == null)
        {
            currentTriangle = new List<Mesh>();
        }

        patches.RemoveAll(x => x == null);
    }


    void OnDrawGizmosSelected()
    {
        if (!Edit) return;
        var colorCache = Gizmos.color;
        Gizmos.color = Color.blue;
        foreach (var patch in patches.Where(x => x.Data.visible))
        {
            foreach (var t in patch.Data.triangles)
            {
                Gizmos.DrawMesh(t);
            }
        }

        Gizmos.color = colorCache;

        Gizmos.color = Color.gray;
        if (skinned)
        {
            Gizmos.DrawWireMesh(selectingVis, 0, transform.position, Quaternion.identity, Vector3.one * 1.1f);

            Gizmos.DrawMesh(selectingVis, 0, transform.position, Quaternion.identity);
        }
        else
        {
            Gizmos.DrawWireMesh(selectingVis, 0);

            Gizmos.DrawMesh(selectingVis, 0);
        }

        Gizmos.color = Color.green;
        foreach (var t in currentTriangle)
        {
            Gizmos.DrawMesh(t);
        }
    }

    public void SavePatch()
    {
        if (currentTriangle.Count == 0) return;
        if (patches == null)
        {
            patches = new List<FurPatchObject>();
        }

        //if editing existing patch
        if (patches.Any(x => x.Data == currentPatchData))
        {
            patches.First(x => x.Data == currentPatchData).Data = currentPatchData;
            return;
        }

        var newPatch = new GameObject("Patch " + patches.Count).AddComponent<FurPatchObject>();
        newPatch.transform.SetParent(transform);
        newPatch.Data = new FurPatchData()
        {
            triangles = currentTriangle,
            push = currentPatchData.push,
            layers = currentPatchData.layers,
            skinned = skinned,
            original = skinnedOriginal
        };
        newPatch.DrawPatch();
        patches.Add(newPatch);
        currentTriangle = new List<Mesh>();
    }

    public void DrawPatches()
    {
        foreach (var patch in patches)
        {
            patch.DrawPatch();
        }
    }

    public void SetTriangle(int index)
    {
        currentTriangle = new List<Mesh>();
        AddTriangle(index);
    }

    public void AddTriangle(int cache)
    {
        var t = new Mesh();
        Mesh sharedMesh = null;
        if (skinned)
        {
            sharedMesh = SkinnedCopy.sharedMesh;
        }
        else
        {
            sharedMesh = Copy.sharedMesh;
        }


        t.vertices = new[]
        {
            sharedMesh.vertices[sharedMesh.triangles[cache * 3]],
            sharedMesh.vertices[sharedMesh.triangles[cache * 3 + 1]],
            sharedMesh.vertices[sharedMesh.triangles[cache * 3 + 2]]
        };

        t.normals = new[]
        {
            sharedMesh.normals[sharedMesh.triangles[cache * 3]],
            sharedMesh.normals[sharedMesh.triangles[cache * 3 + 1]],
            sharedMesh.normals[sharedMesh.triangles[cache * 3 + 2]]
        };

        t.uv = new[]
        {
            sharedMesh.uv[sharedMesh.triangles[cache * 3]],
            sharedMesh.uv[sharedMesh.triangles[cache * 3 + 1]],
            sharedMesh.uv[sharedMesh.triangles[cache * 3 + 2]]
        };

        t.tangents = new[]
        {
            sharedMesh.tangents[sharedMesh.triangles[cache * 3]],
            sharedMesh.tangents[sharedMesh.triangles[cache * 3 + 1]],
            sharedMesh.tangents[sharedMesh.triangles[cache * 3 + 2]]
        };

        if (sharedMesh.boneWeights.Length > 0)
        {
            t.boneWeights = new[]
            {
                sharedMesh.boneWeights[sharedMesh.triangles[cache * 3]],
                sharedMesh.boneWeights[sharedMesh.triangles[cache * 3 + 1]],
                sharedMesh.boneWeights[sharedMesh.triangles[cache * 3 + 2]]
            };
        }

        if (sharedMesh.colors.Length > 0)
            t.colors = new[]
            {
                sharedMesh.colors[sharedMesh.triangles[cache * 3]],
                sharedMesh.colors[sharedMesh.triangles[cache * 3 + 1]],
                sharedMesh.colors[sharedMesh.triangles[cache * 3 + 2]]
            };
        else
        {
            t.colors = new[]
            {
                Color.white, Color.white, Color.white
            };
        }

        if (currentTriangle.Any(x => x.vertices.SequenceEqual(t.vertices)))
        {
            currentTriangle.Remove(currentTriangle.First(x => x.vertices.SequenceEqual(t.vertices)));
            return;
        }


        t.triangles = new[] {0, 1, 2};
        //t.normals = new[] {Vector3.up, Vector3.up, Vector3.up,};
        currentTriangle.Add(t);
    }
}