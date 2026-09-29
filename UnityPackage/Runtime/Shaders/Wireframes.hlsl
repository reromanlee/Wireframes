#ifndef REROMANLEE_WIREFRAMES_INCLUDED
#define REROMANLEE_WIREFRAMES_INCLUDED

// Skinning for Wireframes meshes, usable from any render pipeline. Every vertex stores its position relative to one
// bone and that bone's index in TEXCOORD0; the package uploads each bone's local-to-world matrix into
// _WireframesBones once per frame, as three texels (the matrix rows) per bone and 256 bones per texture row.
// Bone 0 is the identity, which stands for world space.

#define WIREFRAMES_BONES_PER_ROW 256u

Texture2D<float4> _WireframesBones;

// World position of a vertex stored relative to its bone.
float3 WireframesSkin(float3 position, float bone)
{
    uint slot = (uint)(bone + 0.5);
    int3 texel = int3((slot % WIREFRAMES_BONES_PER_ROW) * 3u, slot / WIREFRAMES_BONES_PER_ROW, 0);
    float4 local = float4(position, 1.0);
    return float3(
        dot(_WireframesBones.Load(texel), local),
        dot(_WireframesBones.Load(texel + int3(1, 0, 0)), local),
        dot(_WireframesBones.Load(texel + int3(2, 0, 0)), local));
}

// Vertex colors reach shaders unconverted, so in linear color space they are converted here to look the same as that
// color on a material.
half3 WireframesColor(half3 color)
{
#if defined(UNITY_COLORSPACE_GAMMA)
    return color;
#else
    return color * (color * (color * 0.305306011h + 0.682171111h) + 0.012522878h);
#endif
}

#endif
