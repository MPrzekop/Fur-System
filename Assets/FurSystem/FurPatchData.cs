using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FurPatchData
{
    [HideInInspector] public List<Mesh> triangles;


    public bool visible;
    
    public float push;
    public bool meshPush;
[Range(1,100)]
    public int layers;
    [Range(0, 1f)] public float pushFadeToValue = 1;

    [Range(0, 1f)] public float edgeFadeToValue = 1;
    public float edgeFadeDistance = 0.1f;


    public Vector3 endOffset;
    public bool meshOffset;

    public bool removeDuplicateVertices;

    public bool drawInstanced = false;

    public Matrix4x4[] transforms;
    public Mesh initialLayer;

    public Material material;

    public bool skinned;
    public SkinnedMeshRenderer original;
}