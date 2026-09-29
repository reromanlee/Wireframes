using System;
using Unity.Collections;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// The float texture that shaders read bone matrices from: three texels per bone, the rows of its local-to-world
    /// matrix, and <see cref="BonesPerRow"/> bones per texture row, as Wireframes.hlsl expects. A float texture works on
    /// every platform, WebGL included, where structured buffers are not available to vertex shaders.
    /// </summary>
    internal sealed class BoneTexture : IDisposable
    {
        internal const int BonesPerRow = 256;
        private const int TexelsPerBone = 3;
        // Four 32-bit floats.
        private const int TexelSize = 16;

        internal static readonly int PropertyId = Shader.PropertyToID("_WireframesBones");

        private Texture2D _texture;

        internal Texture2D Texture
        {
            get => _texture;
        }

        /// <summary>
        /// Bytes the texture takes, on the GPU and again on the CPU, where Unity keeps a copy that each upload writes into.
        /// </summary>
        internal long Memory
        {
            get => _texture != null ? (long)_texture.width * _texture.height * TexelSize : 0;
        }

        /// <summary>
        /// Writes every slot's matrix and uploads the texture when a matrix changed or the texture had to grow. Returns
        /// true when a new texture was created, which the renderers then have to be given.
        /// </summary>
        internal bool Upload(BoneRegistry bones)
        {
            bool created = EnsureCapacity(bones.End);
            if (!created && !bones.HasChanges)
            {
                return false;
            }
            using (WireframesMarkers.UploadBones.Auto())
            {
                NativeArray<Vector4> texels = _texture.GetPixelData<Vector4>(0);
                Matrix4x4[] matrices = bones.Matrices;
                int end = bones.End;
                for (int slot = 0; slot < end; slot++)
                {
                    Matrix4x4 matrix = matrices[slot];
                    int texel = slot / BonesPerRow * BonesPerRow * TexelsPerBone + slot % BonesPerRow * TexelsPerBone;
                    texels[texel] = new Vector4(matrix.m00, matrix.m01, matrix.m02, matrix.m03);
                    texels[texel + 1] = new Vector4(matrix.m10, matrix.m11, matrix.m12, matrix.m13);
                    texels[texel + 2] = new Vector4(matrix.m20, matrix.m21, matrix.m22, matrix.m23);
                }
                _texture.Apply(false, false);
            }
            bones.ClearChanges();
            return created;
        }

        public void Dispose()
        {
            UnityObjects.Destroy(_texture);
            _texture = null;
        }

        /// <returns>True when a new texture was created.</returns>
        private bool EnsureCapacity(int slots)
        {
            int rows = Math.Max(1, (slots + BonesPerRow - 1) / BonesPerRow);
            if (_texture != null && _texture.height >= rows)
            {
                return false;
            }
            int height = _texture != null ? _texture.height : 1;
            while (height < rows)
            {
                height *= 2;
            }
            UnityObjects.Destroy(_texture);
            _texture = new Texture2D(BonesPerRow * TexelsPerBone, height, TextureFormat.RGBAFloat, false, true)
            {
                name = "Wireframes Bones",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            return true;
        }
    }
}
