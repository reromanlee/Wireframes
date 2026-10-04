# Performance

What Wireframes costs, measured, and the design behind the numbers. Tips to keep it light are at the end.

### How it stays fast

1. **Moving shapes costs one matrix per bone.** Right before rendering, the package reads each bone's local-to-world matrix and uploads all of them in one float texture, and the vertex shader moves every vertex. A bone costs the same whether one shape or a thousand follow it.
2. **Edits cost only what they change.** Each edited shape is written once per frame, however many of its properties changed, and only the changed ranges of the buffers are uploaded.
3. **Steady frames allocate nothing,** which tests enforce for frames that move bones, edit shapes, hide and show them, or change nothing.
4. **Draw calls stay few.** Shapes share chunks, meshes of up to 65,535 vertices, and each chunk is one draw call per camera, or two with `WireframeOcclusion.Fade`. Nothing is created per shape or per bone: no GameObjects, no components.
5. **Work is spread over frames.** Buffers double when full, freed space goes to the next shape of the same size, and a chunk is compacted once freed space fills half of it, one chunk per frame at most.

### CPU time

Measured by the package's performance tests in the Editor, on an Intel Core i7-12700K, as the median of 10 frames. Each row is one frame: its edits plus the flush that uploads them. The Editor runs scripts on Mono, slower than IL2CPP player builds.

| Frame, on 100 bones | Unity 6000.5 | Unity 2022.3 |
|---|---|---|
| Nothing changed, 10,000 lines | 24 µs | 22 µs |
| Every bone moved, 10,000 lines | 78 µs | 95 µs |
| 1,000 scattered line color edits | 0.26 ms | 0.30 ms |
| 100 scattered sphere radius edits | 0.93 ms | 1.07 ms |
| Dispose half of 10,000 lines | 0.49 ms | 0.44 ms |
| Dispose half of 1,000 spheres | 1.46 ms | 1.47 ms |
| Create 10,000 lines and upload them | 11.3 ms | 14.7 ms |
| Create 1,000 spheres, 96,000 vertices, and upload them | 17.9 ms | 19.0 ms |

Frames that change nothing, move bones or edit shapes allocate nothing. Creating shapes allocates their objects, about 2 per line and 2 per sphere, and disposing thousands of shapes at once allocates a handful of times as internal lists grow.

### Devices

The Stress Test sample, 10,000 lines, 1,000 boxes and 2,000 other shapes on 100 orbiting bones, in builds made with Unity 6000.5. Times are whole frames, from Unity's `FrameTimingManager`.

| Device | Graphics API | Main thread | GPU |
|---|---|---|---|
| Desktop, Intel Core i7-12700K, NVIDIA RTX 4070 Ti | Direct3D 12 | 0.40 ms | 0.46 ms |
| The same desktop, in a Chromium browser | WebGL 2 | 0.49 ms | Not reported |
| Poco X3 NFC, Snapdragon 732G, Adreno 618 | Vulkan | 4.24 ms | 5.64 ms |

### Compared with skinned meshes

Up to 1.0, shapes were drawn by a `SkinnedMeshRenderer`. Before 2.0 replaced it with the package's own shader skinning, a prototype compared the two on the same scenes, alternating between them:

1. **WebGL 2**, where Unity skins meshes on the CPU: frames took 0.6 ms of main thread instead of 1.0 ms at 114,000 vertices, and 0.7 ms instead of 2.2 ms at 454,000. These are rough, from a few samples each.
2. **Windows**: the same CPU time, and 0.10 ms of GPU time instead of 0.15 ms at 114,000 vertices, and 0.21 ms instead of 0.27 ms at 454,000.
3. **Android**, on the Poco X3 NFC with Vulkan: the same GPU time, and 72.9 frames per second instead of 70.3 at 454,000 vertices.

### Memory

1. **On the GPU,** a vertex takes 20 bytes, for its position, color and bone index. An edge takes 4 bytes of indices, or 8 in a chunk made for a shape of more than 65,535 vertices, and a bone takes 48 bytes of the bone texture, which holds 256 bones per 12 KB row.
2. **On the CPU,** a vertex takes the same 20 bytes, an edge about 24 bytes with its bookkeeping, and the bone texture as much as on the GPU; shapes and bones add a few bytes each.
3. **Buffers start small,** at 256 vertices and 128 edges per chunk, and double as needed. Memory past the reserved capacity is given back as shapes are disposed, and a chunk that stays empty for 5 seconds is released.

| Shape, at the default resolution | Vertices | Edges |
|---|---|---|
| Line | 2 | 1 |
| Polyline of n points | n | n - 1, or n when closed |
| Box | 8 | 12 |
| Rectangle | 4 | 4 |
| Rounded rectangle | 36 | 36 |
| Circle, ellipse | 32 | 32 |
| Star with 5 points | 10 | 10 |
| Sphere, ellipsoid | 96 | 96 |
| Spiked sphere with 4, 6 or 8 spikes | 8, 14 or 14 | 18, 36 or 36 |
| Spiked sphere with 12 or 20 spikes | 32 | 90 |
| Cylinder | 64 | 68 |
| Cone | 33 | 36 |
| Capsule | 122 | 132 |
| Stadium | 34 | 34 |
| Frustum with n sides | 2n | 3n |
| Pyramid | 5 | 8 |
| Text | About 7 per character of the Default Font, rounded up to a power of two, at least 16 | About 5 per character of the Default Font |
| Symbol | 16 for most Default Symbols, and 32 or 64 for round ones | 1 to 34 for the Default Symbols |

### Tips

1. **Move bones rather than editing points.** A moving bone uploads its 48 bytes, while an edit rewrites every vertex of its shape.
2. **Reserve capacity for a known load** with `VertexCapacity` and `EdgeCapacity`, so buffers never grow mid-game.
3. **Hide shapes that come back** with `IsVisible` instead of disposing and creating them again.
4. **Lower `segmentCount`** for small or distant round shapes.
5. **Rewrite a text with `SetText`** rather than creating another: it keeps room to grow, so most changes rewrite it in place without allocating.
6. **Group shapes in few containers.** Each container has its own bone texture and draw calls.
7. **Use `Fade` where it helps,** since it doubles a container's draw calls.
8. **Hide whole containers that are off screen.** Shapes follow arbitrary Transforms, so their meshes have fixed bounds of ±1,000 km and are never frustum-culled.
