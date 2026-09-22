using reromanlee.Wireframes.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class MeshProxy : IMeshProxy
    {
        private const string RootBoneName = "RootBone";
        private const string ShaderName = "reromanlee/Wireframes/VertexColors";

        private readonly BoneWeight RootBoneWeight = new()
        {
            boneIndex0 = 0,
            weight0 = 1.0f
        };

        private readonly GameObject _proxyObject;
        private readonly SkinnedMeshRenderer _skinnedMeshRenderer;
        private readonly Mesh _mesh;
        private readonly GameObject _rootBoneObject;

        private Vector3[] _meshVertices = Array.Empty<Vector3>();
        private Color[] _meshColors = Array.Empty<Color>();
        private BoneWeight[] _meshBoneWeights = Array.Empty<BoneWeight>();
        private Matrix4x4[] _meshBindposes = Array.Empty<Matrix4x4>();

        private readonly List<ILine> _lineCollection = new();
        private readonly List<IBox> _boxCollection = new();

        public MeshProxy()
        {
            // Create a new GameObject to serve as the proxy for the mesh and add a SkinnedMeshRenderer component to it.
            _proxyObject = new GameObject(nameof(MeshProxy));
            _skinnedMeshRenderer = _proxyObject.AddComponent<SkinnedMeshRenderer>();

            // Set up the material with the custom shader.
            Shader shader = Shader.Find(ShaderName);
            Material material = new(shader);
            _skinnedMeshRenderer.sharedMaterial = material;

            // Set up the mesh and assign it to the SkinnedMeshRenderer.
            _mesh = new();
            _skinnedMeshRenderer.sharedMesh = _mesh;

            // Configure the SkinnedMeshRenderer to update even when offscreen and disable occlusion culling.
            _skinnedMeshRenderer.updateWhenOffscreen = true;
            _skinnedMeshRenderer.allowOcclusionWhenDynamic = false;

            // Set up the root bone for the SkinnedMeshRenderer.
            _rootBoneObject = new GameObject(RootBoneName);
            _rootBoneObject.transform.SetParent(_proxyObject.transform);
            _rootBoneObject.transform.position = Vector3.zero;
            _skinnedMeshRenderer.bones = new[] { RootBone };
            _skinnedMeshRenderer.rootBone = RootBone;

            // Initialize the mesh with the default bindpose.
            _meshBindposes = new[] { Matrix4x4.identity };
        }

        public Transform RootBone
        {
            get => _rootBoneObject.transform;
        }

        private int LineCount
        {
            get => _lineCollection.Count;
        }

        private int VertexCount
        {
            get
            {
                return _lineCollection.Count * 2;
            }
        }

        private void OnLinePositionChange(Line line)
        {
            int lineIndex = _lineCollection.IndexOf(line);
            int vertexIndexA = lineIndex * 2;
            int vertexIndexB = vertexIndexA + 1;
            _meshVertices[vertexIndexA] = line.PositionA;
            _meshVertices[vertexIndexB] = line.PositionB;
            _mesh.vertices = _meshVertices;
        }

        private void OnLineColorChange(Line line)
        {
            int lineIndex = _lineCollection.IndexOf(line);
            int vertexIndexA = lineIndex * 2;
            int vertexIndexB = vertexIndexA + 1;
            _meshColors[vertexIndexA] = line.ColorA;
            _meshColors[vertexIndexB] = line.ColorB;
            _mesh.colors = _meshColors;
        }

        // TODO: implement cleanup of unnecessary bones.
        private void OnLineBoneChange(Line line)
        {
            ChangeBones(line);
        }

        public void AddLine(Line line)
        {
            _lineCollection.Add(line);

            // Subscribe to line change events.
            line.OnPositionChange += OnLinePositionChange;
            line.OnColorChange += OnLineColorChange;
            line.OnBoneChange += OnLineBoneChange;

            // Append line to renderer.
            AppendToMesh(line);
        }

        public void RemoveLine(Line line)
        {
            _lineCollection.Remove(line);

            // Unsubscribe from line change events.
            line.OnPositionChange -= OnLinePositionChange;
            line.OnColorChange -= OnLineColorChange;
            line.OnBoneChange -= OnLineBoneChange;

            // Recalculate whole mesh in case of removing elements.
            RecalculateAll();
        }

        public void AddBox(Box box)
        {
            _boxCollection.Add(box);

            // Append box to renderer.
            AppendToMesh(box);
        }

        public void RemoveBox(Box box)
        {
            _boxCollection.Remove(box);

            // Recalculate whole mesh in case of removing elements.
            RecalculateAll();
        }

        private void AppendToMesh(Line line)
        {
            // Append line vertices and colors to mesh arrays.
            _meshVertices = _meshVertices.Append(line.PositionA, line.PositionB);
            _meshColors = _meshColors.Append(line.ColorA, line.ColorB);

            // Assign vertex and color arrays to mesh.
            _mesh.vertices = _meshVertices;
            _mesh.colors = _meshColors;

            // Create index array.
            int vertexCount = _meshVertices.Length;
            int[] indices = new int[vertexCount];

            // Assign index array.
            for (int x = 0; x < vertexCount; x++)
            {
                indices[x] = x;
            }

            // Assign index array to mesh.
            _mesh.SetIndices(indices, MeshTopology.Lines, 0);

            // Append empty bone weights to mesh.
            _meshBoneWeights = _meshBoneWeights.Append(RootBoneWeight, RootBoneWeight);
            _mesh.boneWeights = _meshBoneWeights;
        }

        private void AppendToMesh(Box box)
        {
            // Append box vertices and colors to mesh arrays.
            _meshVertices = _meshVertices.Append(box.Positions);
            _meshColors = _meshColors.Append(box.Colors);

            _mesh.vertices = _meshVertices;
            _mesh.colors = _meshColors;

            // Create index array.
            int vertexCount = _meshVertices.Length;
            int[] indices = new int[vertexCount];

            // Assign index array.
            for (int x = 0; x < vertexCount; x++)
            {
                indices[x] = x;
            }

            // Assign index array to mesh.
            _mesh.SetIndices(indices, MeshTopology.Lines, 0);

            // Create bone weights array.
            BoneWeight[] boneWeights = new BoneWeight[box.Positions.Length];
            for (int x = 0; x < boneWeights.Length; x++)
            {
                boneWeights[x] = RootBoneWeight;
            }

            // Append empty bone weights to mesh.
            _meshBoneWeights = _meshBoneWeights.Append(boneWeights);
            _mesh.boneWeights = _meshBoneWeights;
        }

        /// <summary>This method edit existing array elements, it does not append new elements, except unique bone transforms.</summary>
        private void ChangeBones(Line line)
        {
            // Get bones reference from renderer.
            Transform[] bones = _skinnedMeshRenderer.bones;

            // Try to find bone indices in bones array.
            int boneIndexA = Array.IndexOf(bones, line.BoneA);
            int boneIndexB = Array.IndexOf(bones, line.BoneB);

            // Append both bones if they don't exist.
            if (boneIndexA == -1 && boneIndexB == -1)
            {
                // Append only one bone if both are the same.
                if (line.BoneA == line.BoneB)
                {
                    boneIndexA = boneIndexB = bones.Length;
                    bones = bones.Append(line.BoneA);
                    _meshBindposes = _meshBindposes.Append(line.BoneA.worldToLocalMatrix * RootBone.localToWorldMatrix);
                }
                // Append both bones if they are different.
                else
                {
                    boneIndexA = bones.Length;
                    boneIndexB = bones.Length + 1;
                    bones = bones.Append(line.BoneA, line.BoneB);
                    _meshBindposes = _meshBindposes.Append(
                        line.BoneA.worldToLocalMatrix * RootBone.localToWorldMatrix,
                        line.BoneB.worldToLocalMatrix * RootBone.localToWorldMatrix
                    );
                }
            }
            // Append bone A if not exists.
            else if (boneIndexA == -1)
            {
                boneIndexA = bones.Length;
                bones = bones.Append(line.BoneA);
                _meshBindposes = _meshBindposes.Append(line.BoneA.worldToLocalMatrix * RootBone.localToWorldMatrix);
            }
            // Append bone B if not exists.
            else if (boneIndexB == -1)
            {
                boneIndexB = bones.Length;
                bones = bones.Append(line.BoneB);
                _meshBindposes = _meshBindposes.Append(line.BoneB.worldToLocalMatrix * RootBone.localToWorldMatrix);
            }

            // Assign new bones array to renderer.
            _skinnedMeshRenderer.bones = bones;

            // Assign bindposes array to mesh.
            _mesh.bindposes = _meshBindposes;

            // Get line index from line collection.
            int lineIndex = _lineCollection.IndexOf(line);
            int vertexIndexA = lineIndex * 2;
            int vertexIndexB = vertexIndexA + 1;

            // Assign bone indices to bone weights array.
            _meshBoneWeights[vertexIndexA].boneIndex0 = boneIndexA;
            _meshBoneWeights[vertexIndexB].boneIndex0 = boneIndexB;

            // Assign bone weights array to mesh.
            _mesh.boneWeights = _meshBoneWeights;
        }

        private void RecalculateMesh()
        {
            int lineCount = LineCount;
            int vertexCount = VertexCount;

            // Clear already existing mesh.
            _mesh.Clear();

            // Create vertex and color arrays.
            Vector3[] vertices = new Vector3[vertexCount];
            Color[] colors = new Color[vertexCount];

            // Assign vertex and color arrays.
            for (int x = 0; x < lineCount; x++)
            {
                // Assign vertex array.
                vertices[x * 2] = _lineCollection[x].PositionA;
                vertices[x * 2 + 1] = _lineCollection[x].PositionB;

                // Assign color array.
                colors[x * 2] = _lineCollection[x].ColorA;
                colors[x * 2 + 1] = _lineCollection[x].ColorB;
            }

            // Assign vertex and color arrays to mesh.
            _mesh.vertices = vertices;
            _mesh.colors = colors;

            // Create index array.
            int[] indices = new int[vertexCount];

            // Assign index array.
            for (int x = 0; x < vertexCount; x++)
            {
                indices[x] = x;
            }

            // Assign index array to mesh.
            _mesh.SetIndices(indices, MeshTopology.Lines, 0);
        }

        private void RecalculateBones()
        {
            int lineCount = LineCount;
            int vertexCount = VertexCount;

            // Create bones and bone weight arrays.
            List<Transform> uniqueBones = new() { RootBone };
            Dictionary<Transform, int> boneMapping = new();
            BoneWeight[] boneWeights = new BoneWeight[vertexCount];

            // Assign bones and bone weight arrays.
            for (int x = 0; x < lineCount; x++)
            {
                Transform boneA = _lineCollection[x].BoneA;
                Transform boneB = _lineCollection[x].BoneB;

                // Get or add bone A to unique bones collection.
                if (boneMapping.TryGetValue(boneA, out int boneIndexA) == false)
                {
                    boneIndexA = uniqueBones.Count;
                    uniqueBones.Add(boneA);
                    boneMapping.Add(boneA, boneIndexA);
                }

                // Get or add bone B to unique bones collection.
                if (boneMapping.TryGetValue(boneB, out int boneIndexB) == false)
                {
                    boneIndexB = uniqueBones.Count;
                    uniqueBones.Add(boneB);
                    boneMapping.Add(boneB, boneIndexB);
                }

                // Assign bone weight array.
                boneWeights[x * 2] = new BoneWeight()
                {
                    boneIndex0 = boneIndexA,
                    weight0 = 1.0f
                };
                boneWeights[x * 2 + 1] = new BoneWeight()
                {
                    boneIndex0 = boneIndexB,
                    weight0 = 1.0f
                };
            }

            // Assign bones to renderer.
            _skinnedMeshRenderer.bones = uniqueBones.ToArray();

            // Assign bone weight array to mesh.
            _mesh.boneWeights = boneWeights;

            // Create bindpose array.
            Matrix4x4[] bindposes = new Matrix4x4[uniqueBones.Count];

            // Assign bindpose array.
            for (int x = 0; x < uniqueBones.Count; x++)
            {
                bindposes[x] = uniqueBones[x].worldToLocalMatrix * RootBone.localToWorldMatrix;
            }

            // Assign bindpose array to mesh.
            _mesh.bindposes = bindposes;
        }

        private void RecalculateAll()
        {
            RecalculateMesh();
            RecalculateBones();
        }
    }
}