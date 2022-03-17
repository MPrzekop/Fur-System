using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

[System.Serializable]
public class FurPatchObject : MonoBehaviour
{
    [SerializeField] private FurPatchData data;

    public FurPatchData Data
    {
        get => data;
        set => data = value;
    }


    private void OnValidate()
    {
        DrawPatch();
    }

    private Mesh GenerateInitialLayer()
    {
        var layerMesh = new List<Mesh>();
        //Layer 0
        float offset = (Data.push / Data.layers) * (+1);

        foreach (var triangle in Data.triangles)
        {
            var copy = Instantiate(triangle);
            var newVertices = new Vector3[copy.vertexCount];
            var newColors = new Color[copy.colors.Length];
            var offsetPosition = Vector3.Lerp(Vector3.zero, Data.endOffset, ((float) 0 / data.layers));
            var offsetAlongNormals = Vector3.zero;
            for (int j = 0; j < copy.vertexCount; j++)
            {
                offsetAlongNormals = (triangle.normals[j] * offset);
                newVertices[j] = copy.vertices[j] + (Data.meshPush ? offsetAlongNormals : Vector3.zero) +
                                 (Data.meshOffset
                                     ? offsetPosition
                                     : Vector3.zero);
            }

            for (int j = 0; j < copy.colors.Length; j++)
            {
                offsetAlongNormals = (triangle.normals[j] * offset);
                var color = new Color();
                //set vertex offset as vertex color 
                for (int c = 0; c < 3; c++)
                {
                    color[c] = offsetAlongNormals[c] + offsetPosition[c];
                }

                //set alpha as fade value
                color.a = Mathf.Lerp(1, Data.pushFadeToValue, (((float) 0 / data.layers)));
                newColors[j] = color;
            }

            copy.colors = newColors;
            copy.vertices = newVertices;
            layerMesh.Add(copy);
        }

        var m = new Mesh();
        m.CombineMeshes(layerMesh);
        if (Data.removeDuplicateVertices)
            m = m.RemoveDoubles();

        var edgeVerts = m.GetEdgeVertices();
        var colors = m.colors;
        for (var index = 0; index < m.vertices.Length; index++)
        {
            var vert = m.vertices[index];
            float distance = edgeVerts.Min(x => Vector3.Distance(m.vertices[x], vert));
            colors[index].a = Mathf.Lerp(data.edgeFadeToValue, colors[index].a,
                Mathf.Min(1, distance / Data.edgeFadeDistance));
        }

        m.colors = colors;
        m.CombineMeshes(layerMesh.ToArray());
        return m;
    }

    public void DrawPatch()
    {
        if (Data == null) return;
        var meshesToRender = new List<Mesh>();
        var layerMesh = new List<CombineInstance>();

        Data.initialLayer = GenerateInitialLayer();
        meshesToRender.Add(Data.initialLayer);
        if (Data.drawInstanced)
        {
            Data.transforms = new Matrix4x4[data.layers];
            for (int i = 0; i < data.layers; i++)
            {
                float offset = (Data.push / Data.layers) * (i + 1);
                var offsetPosition = Vector3.Lerp(Vector3.zero, Data.endOffset, ((float) i / data.layers));
                Data.transforms[i] =
                    Matrix4x4.TRS(transform.position + offsetPosition, transform.rotation, Vector3.one);
            }

            Graphics.DrawMeshInstanced(Data.initialLayer, 0, GetComponent<Renderer>().sharedMaterial, Data.transforms,
                Data.layers);
        }

        if (!Data.drawInstanced)
        {
            for (int i = 1; i < Data.layers; i++)
            {
                float offset = (Data.push / Data.layers) * (i + 1);
                var meshCopy = Instantiate(Data.initialLayer);
                var color = meshCopy.colors;
                var vertices = meshCopy.vertices;
                var offsetPosition = Vector3.Lerp(Vector3.zero, Data.endOffset, ((float) i / data.layers));
                var offsetAlongNormals = Vector3.zero;

                for (int j = 0; j < meshCopy.vertexCount; j++)
                {
                    offsetAlongNormals = (meshCopy.normals[j] * offset);


                    vertices[j] = meshCopy.vertices[j] + (Data.meshPush ? offsetAlongNormals : Vector3.zero) +
                                  (Data.meshOffset
                                      ? offsetPosition
                                      : Vector3.zero);

                    for (int c = 0; c < 3; c++)
                    {
                        color[j][c] = offsetAlongNormals[c] + offsetPosition[c];
                    }

                    color[j].a *= Mathf.Lerp(1, Data.pushFadeToValue, (((float) i / data.layers)));
                }

                meshCopy.vertices = vertices;
                meshCopy.colors = color;
                meshesToRender.Add(meshCopy);
            }

            if (!Data.skinned)
            {
                if (GetComponent<SkinnedMeshRenderer>() != null)
                {
                    DestroyImmediate(GetComponent<SkinnedMeshRenderer>());
                }

                if (GetComponent<MeshFilter>() == null)
                {
                    gameObject.AddComponent<MeshFilter>();
                }

                if (GetComponent<MeshRenderer>() == null)
                {
                    gameObject.AddComponent<MeshRenderer>();
                }

                if (GetComponent<MeshFilter>().sharedMesh == null)
                    GetComponent<MeshFilter>().mesh = new Mesh();

                GetComponent<MeshFilter>().sharedMesh.CombineMeshes(meshesToRender
                    .Select(m => new CombineInstance() {mesh = m, transform = transform.localToWorldMatrix}).ToArray());

                GetComponent<MeshFilter>().sharedMesh.Optimize();
            }
            else
            {
                if (GetComponent<SkinnedMeshRenderer>() == null)
                {
                    gameObject.AddComponent<SkinnedMeshRenderer>();
                }

                if (GetComponent<MeshFilter>() != null)
                {
                    DestroyImmediate(GetComponent<MeshFilter>());
                }

                if (GetComponent<MeshRenderer>() != null)
                {
                    DestroyImmediate(GetComponent<MeshRenderer>());
                }

                if (GetComponent<SkinnedMeshRenderer>().sharedMesh == null)
                    GetComponent<SkinnedMeshRenderer>().sharedMesh = new Mesh();

                GetComponent<SkinnedMeshRenderer>().sharedMesh.CombineMeshes(meshesToRender);
                GetComponent<SkinnedMeshRenderer>().bones = Data.original.bones;
                GetComponent<SkinnedMeshRenderer>().sharedMesh.bindposes = Data.original.sharedMesh.bindposes;
                GetComponent<SkinnedMeshRenderer>().rootBone = Data.original.rootBone;
            }
        }
    }

    void Update()
    {
        if (Data.drawInstanced)
        {
            Graphics.DrawMeshInstanced(Data.initialLayer, 0, Data.material, Data.transforms,
                Data.layers);
        }
    }
}

public static class MeshExtension
{
    public static int GetDuplicate(this Mesh m, Vector3 vertex, Vector2 UV, Vector3 normal, Color color)
    {
        for (int i = 0; i < m.vertexCount; i++)
        {
            if (m.vertices[i].Equals(vertex))
            {
                if (m.uv[i].Equals(UV) && m.normals[i].Equals(normal))
                {
                    return i;
                }
                else
                {
                    continue;
                }
            }
            else
            {
                continue;
            }
        }

        return -1;
    }

    public static Mesh RemoveDoubles(this Mesh m)
    {
        var vertices = m.vertices.ToList();
        var uvs = m.uv.ToList();
        var normals = m.normals.ToList();
        var triangles = m.triangles;
        var tangents = m.tangents.ToList();
        var colors = m.colors.ToList();
        var boneWeights = new List<BoneWeight>();
        Dictionary<int, int> replacements = new Dictionary<int, int>();
        for (int i = 0; i < vertices.Count(); i++)
        {
            var duplicateIndex = m.GetDuplicate(m.vertices[i], m.uv[i], m.normals[i], m.colors[i]);
            if (duplicateIndex == i) continue;
            if (duplicateIndex == -1) continue;
            replacements.Add(i, duplicateIndex);
        }

        vertices.Clear();
        normals.Clear();
        uvs.Clear();
        tangents.Clear();
        colors.Clear();
        int currentVertIndex = 0;
        for (int i = 0; i < m.vertexCount; i++)
        {
            if (replacements.ContainsKey(i))
            {
            }
            else
            {
                vertices.Add(m.vertices[i]);
                normals.Add(m.normals[i]);
                uvs.Add(m.uv[i]);
                tangents.Add(m.tangents[i]);
                colors.Add(m.colors[i]);
                if (m.boneWeights.Length > 0)
                    boneWeights.Add(m.boneWeights[i]);
                currentVertIndex++;
            }
        }

        var ret = new Mesh()
        {
            vertices = vertices.ToArray(), normals = normals.ToArray(), uv = uvs.ToArray(),
            tangents = tangents.ToArray(), colors = colors.ToArray(), boneWeights = boneWeights.ToArray()
        };
        for (int i = 0; i < triangles.Length; i++)
        {
            int dup = ret.GetDuplicate(m.vertices[triangles[i]], m.uv[triangles[i]], m.normals[triangles[i]],
                m.colors[triangles[i]]);
            if (dup == -1)
            {
                Debug.LogError("removing doubles failed");
                return m;
            }

            triangles[i] = dup;
        }

        ret.triangles = triangles;
        return ret;
    }

    public static void CombineMeshes(this Mesh m, IEnumerable<Mesh> data)
    {
        var verts = new Vector3[data.Sum(x => x.vertexCount)];
        var normals = new Vector3[verts.Length];
        var uvs = new Vector2[verts.Length];
        var colors = new Color[verts.Length];
        var triangles = new int[data.Sum(x => x.triangles.Length)];
        var boneWeights = new BoneWeight[verts.Length];

        int iterator = 0;
        foreach (var mesh in data)
        {
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                verts[iterator + i] = mesh.vertices[i];
                normals[iterator + i] = mesh.normals[i];
                uvs[iterator + i] = mesh.uv[i];
                colors[iterator + i] = mesh.colors[i];
                if (mesh.boneWeights.Length > 0)
                    boneWeights[iterator + i] = mesh.boneWeights[i];
            }

            for (int i = 0; i < mesh.triangles.Length; i++)
            {
                triangles[iterator + i] = mesh.triangles[i] + iterator;
            }

            iterator += mesh.vertexCount;
        }

        m.vertices = verts;
        m.normals = normals;
        m.uv = uvs;
        m.colors = colors;
        m.triangles = triangles;
        m.boneWeights = boneWeights;
    }

    class edge : IEquatable<edge>
    {
        public HashSet<int> edgeVerts;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return edgeVerts.SetEquals((obj as edge).edgeVerts);
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public bool Equals(edge other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return edgeVerts.SetEquals(other.edgeVerts);
        }
    }


    /// <summary>
    /// Returns vertices attached to edges that are used only by one triangle, effectively giving "hard" edge of a mesh
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    public static List<int> GetEdgeVertices(this Mesh m)
    {
        var ret = new List<int>();
        List<edge> edges = new List<edge>();

        for (int i = 0; i < m.triangles.Length; i += 3)
        {
            edges.Add(new edge()
            {
                edgeVerts = new HashSet<int>()
                {
                    m.triangles[i], m.triangles[i + 1]
                }
            });
            edges.Add(new edge()
            {
                edgeVerts = new HashSet<int>()
                {
                    m.triangles[i + 1], m.triangles[i + 2]
                }
            });
            edges.Add(new edge()
            {
                edgeVerts = new HashSet<int>()
                {
                    m.triangles[i + 2], m.triangles[i]
                }
            });
        }

        var distinctEdges = edges.Where(x => edges.Count(y => y.Equals(x)) == 1);
        foreach (var edge in distinctEdges)
        {
            ret.AddRange(edge.edgeVerts);
        }

        return ret.Distinct().ToList();
    }
}