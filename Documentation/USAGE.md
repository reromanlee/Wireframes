# Usage

How to use Wireframes, from the first container to custom shaders. What each part does is in [FEATURES.md](FEATURES.md).

### Containers

`new WireframeContainer()` creates a GameObject named **Wireframes** in the active scene, which draws every shape created in the container.

1. The container lives until you call `Dispose()` or the scene it was created in unloads. Either way, every shape it created is disposed too, and `IsDisposed` becomes true.
2. `new WireframeContainer(settings)` takes a `WireframeContainerSettings`, read once, so changing it later doesn't affect the container.
3. The container never destroys a material passed in its settings.
4. Create containers and shapes on the main thread, from `Awake`, `OnEnable`, `Start` or later, not from constructors or field initializers. In the Editor and development builds, calls from other threads throw `InvalidOperationException`.

Settings are serializable, so a script can show them in the Inspector:

```csharp
[SerializeField] private WireframeContainerSettings _settings = new();

private void OnEnable()
{
    _wireframes = new WireframeContainer(_settings);
}
```

### Creating shapes

Every shape except lines and polylines has the same kinds of Create methods:

| Kind | Example | Space |
|---|---|---|
| No arguments | `CreateCylinder()` | A white shape of unit size at the world origin: radius 0.5, sizes 1, length 1. |
| The usual form | `CreateCylinder(endA, endB, radius)` | World space. |
| The same with a bone first | `CreateCylinder(bone, localEndA, localEndB, radius)` | The bone's local space. |
| Position and rotation | `CreateCylinder(position, rotation, length, radius)` | World space. |

A few shapes add an obvious extra, such as `CreateCircle(center, normal, radius)` or a stadium with an explicit normal.

Lines and polylines are made of points: they are created from world positions, or from one bone per point, each point sitting at its bone's origin. `CreateLine()` starts with both ends at the world origin.

### Bones and spaces

A **bone** is any Transform in a scene that a shape follows; `null` means world space. A prefab asset can't be a bone and throws `ArgumentException`.

1. Lines and polylines have a bone for every point, because points can go anywhere and still make a line.
2. Every other shape has one bone and moves rigidly with it, like a child Transform (`IRigidShape`). Its position and rotation are relative to that bone, and its sizes are in the bone's units, so it also scales with the bone.
3. `Local*` properties are relative to the bone, or world space without a bone.
4. `World*` properties convert through the bone's current pose, like `Transform.position`.
5. Changing a bone keeps the shape where it is in the world, like reparenting a Transform: points keep their world position, and other shapes also keep their world rotation and size. From then on it moves with the new bone.

**Rule of thumb: create a shape on its bone when you know the bone, or set the bone first and then the position in whichever space you mean.**

| You want | Write |
|---|---|
| A line joining two Transforms | `CreateLine(a, b)` |
| A point at an offset from a bone, such as a sword tip | `line.BoneB = sword; line.LocalPositionB = new Vector3(0f, 1f, 0f);` |
| A point pinned where something was hit, moving with it | `line.BoneA = hitTransform; line.WorldPositionA = hit.point;` |
| A shape that stops following | `shape.Bone = null;` (it stays where it is) |

### A shape's axes

A shape's rotation turns its own axes:

1. **+Y is the normal of flat shapes.** Rectangles, circles, ellipses, stars and stadiums lie in their XZ plane, so with no rotation they lie flat on the ground, or flat around their bone.
2. **+Z is the axis of long shapes** (`IAxialShape`: cylinders, cones, capsules, stadiums, frustums and pyramids). They start at end A, at their position, and run `Length` along +Z to end B. Setting one end keeps the other where it is: the axis turns by the smallest rotation and `Length` follows. Setting the position moves the whole shape.
3. Other shapes are centered on their position.
4. `Quaternion.LookRotation(axis, normal)` is the rotation that points a shape's +Z along `axis` with +Y toward `normal`.
5. A shape created from two points turns so that its +Y stays as close to world up, or its bone's up, as it can. A stadium or ellipse created that way lies as flat as it can.
6. Capsule and stadium ends are the centers of their spheres or circles, as in `Physics.CapsuleCast`. When one sphere holds the other, only the bigger one is drawn.

### Sizes and resolution

1. Sizes are in the bone's units. Changing a shape's bone multiplies them by the ratio of the two bones' scales, so the shape keeps its size in the world, exactly for uniformly scaled bones.
2. Sizes are used as given: a negative size mirrors the shape, and zero collapses it. A box keeps a signed `Size`, so both of its corners come back exactly as they were set.
3. Round shapes take `segmentCount`, the number of straight pieces a full circle is drawn with: 32 by default, from 3 to 1,024. Cylinders, cones, capsules, stadiums and rounded rectangles need a multiple of 4, because their lines meet their rings at quarter points, and arcs use their share of it.
4. Frustums take a `sideCount` from 3 to 1,024 (4 by default), stars a `pointCount` from 3 to 512 (5 by default), and spiked spheres a `spikeCount` of 4, 6, 8, 12 or 20 (12 by default).
5. Those counts, and the number of points of a polyline, are fixed when a shape is created. To change them, create a new shape.

### Colors

1. Lines and polylines have a color per point, blended along the edges. `SetColor` sets them all.
2. Every other shape has one `Color`, which `SetColor` sets too.
3. Colors are stored with 8 bits per channel, so values above 1 are clamped: no HDR glow.
4. In linear color space, the package's shader converts vertex colors so they look the same as that `Color` on a material.
5. Alpha is ignored unless the container's `UseAlpha` setting is on.

### When edits show up

Setters only record the change, and each edited shape is queued once, however many of its properties change. Right before a camera renders, the container writes the queued shapes, uploads what changed and reads its bones, so everything edited or moved earlier in the frame shows up in that frame, whether it happened in `Update`, `LateUpdate`, animation or physics. Changes made after rendering, such as in `WaitForEndOfFrame`, show up in the next frame.

### Visibility

`shape.IsVisible = false` hides a shape without disposing it. It keeps its place, bones and settings, takes edits while hidden, and draws again as it is then when shown. `container.IsVisible = false` hides every shape of the container, and a hidden container does no per-frame work until it is shown again. Hiding and showing allocate nothing, which makes them the cheap way to toggle shapes that come back.

### Destroyed bones

When a bone's GameObject is destroyed, the shapes and points on it stay exactly where they were last drawn, at the same size. The next time you use such a shape, its bone reads as `null` and it continues in world space from there. A move of the bone in the frame it was destroyed, after that frame's render, is lost.

### Disposing shapes

`shape.Dispose()` removes a shape and frees its space for the next shape of the same size. After that, `IsDisposed` is true and every other member throws `ObjectDisposedException`. Disposing again does nothing.

### Edit Mode

Containers work the same in Edit Mode, drawing in the Scene and Game views, which suits previews from `[ExecuteAlways]` scripts:

```csharp
[ExecuteAlways]
public sealed class BoundsPreview : MonoBehaviour
{
    private WireframeContainer _wireframes;

    private void OnEnable()
    {
        _wireframes = new WireframeContainer();
        _wireframes.CreateBox(transform, new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
    }

    private void OnDisable()
    {
        _wireframes.Dispose();
    }
}
```

1. An Edit Mode container is never saved into its scene and never marks it as changed.
2. It is disposed before scripts reload, after which Unity calls `OnEnable` again, and when its scene closes, unless it persists across scenes.
3. Switching between Edit and Play Mode alone never disposes a container, though the script reload that Play Mode starts with by default does.

### Custom materials

A custom material draws a container when its shader moves vertices with `WireframesSkin` from the package's include file. Each vertex arrives relative to its bone, with the bone's index in `TEXCOORD0`, and the package sets the `_WireframesBones` texture on every renderer. `WireframesColor` converts vertex colors in linear color space, as the package's shader does.

```hlsl
#include "Packages/com.reromanlee.wireframes/Runtime/Shaders/Wireframes.hlsl"

struct Attributes
{
    float3 position : POSITION;
    half4 color : COLOR;
    float bone : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    half4 color : COLOR;
};

Varyings Vertex(Attributes input)
{
    Varyings output;
    float3 world = WireframesSkin(input.position, input.bone);
    output.positionCS = mul(UNITY_MATRIX_VP, float4(world, 1.0));
    output.color = half4(WireframesColor(input.color.rgb), input.color.a);
    return output;
}
```

HDRP renders relative to the camera, so there the world position goes through `GetCameraRelativePositionWS` before `TransformWorldToHClip`. The package's own shader, `Runtime/Resources/reromanlee.Wireframes/Unlit.shader`, has complete passes for the Built-in Render Pipeline, URP and HDRP to start from. The `Occlusion` and `UseAlpha` settings apply to the package's material only.

### Statistics and profiling

`container.Statistics` returns what a container holds and the memory it uses, without allocating, so it can be read every frame. `ChunkCount` is also the number of draw calls per camera, or twice that with `WireframeOcclusion.Fade`.

```csharp
_wireframes.CreateLine(_hand, _target);
_wireframes.CreateCircle(_target, Vector3.zero, 1f);
_wireframes.CreateSphere(_target, Vector3.zero, 0.5f).IsVisible = false;

// Shapes: 3 (1 hidden), vertices: 130, edges: 33, bones: 2, chunks: 1, CPU memory: 25 KB, GPU memory: 17 KB
Debug.Log(_wireframes.Statistics);
```

1. In the Profiler, the package's work shows up under markers named `Wireframes.*`, in the Render category.
2. With the `com.unity.profiling.core` package installed, the Profiler counters `Wireframes Shapes`, `Vertices`, `Edges`, `Bones`, `Chunks` and `GPU Memory` add up every container; add them to a module in the Profiler's Module Editor.
3. Warnings and errors start with `[Wireframes]` and are logged once per cause.

### Wireframe camera

Add the **WireframeCamera** component to a Camera to draw everything that camera renders as wireframe, like the Scene view's Wireframe draw mode, while other cameras draw normally. It follows the render pipeline if it changes at runtime, though the frame in which the pipeline switches is drawn without wireframe. It uses `GL.wireframe`, which OpenGL ES and WebGL don't support.

### Samples

Import them from the package's **Samples** tab in the Package Manager.

1. **Shape Gallery** lays out every shape in three rows, each turning with its own bone. Open its **ShapeGallery** scene and enter Play Mode.
2. **Stress Test** spawns 10,000 lines, 1,000 boxes and 2,000 other shapes on 100 orbiting bones. Add the `StressTest` component to an empty GameObject in a scene with a camera, and enter Play Mode with the Profiler open. Raise **Recolor Per Frame** to measure the cost of edits.

### Tests

To run the package's tests in the Test Runner, add the package to `testables` in `Packages/manifest.json`:

```json
"testables": ["com.reromanlee.wireframes"]
```

The timing tests run when the project also has the `com.unity.test-framework.performance` package, and the Profiler counter test when it has `com.unity.profiling.core`.
