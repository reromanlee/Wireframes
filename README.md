# Wireframes for Unity

Wireframe lines and boxes for Unity 6 that follow Transforms at no per-frame CPU cost to your code.

You create a shape once, attach its ends to any Transforms ("bones"), and it moves with them from then on. The GPU does the moving, through the same skinning Unity uses for animated characters. Every shape in a container is one mesh, drawn in one draw call.

## Why not `Debug.DrawLine`?

| | `Debug.DrawLine` / Gizmos | Wireframes |
|---|---|---|
| Moving lines | Call it again every frame, for every line | Create once; bones move them on the GPU |
| CPU cost per frame | Grows with the number of lines | No work from the package while nothing is edited |
| Player builds | Not drawn | Drawn |

## Requirements

- Unity 6 (6000.0) or later.
- The default material is an unlit vertex-color shader, written for and tested with the Built-in Render Pipeline. With URP or HDRP, if it doesn't render, pass your own material to the container; any shader that outputs vertex colors works.

## Installation

In **Window > Package Manager**, choose **+ > Install package from git URL** and enter:

```
https://github.com/reromanlee/Wireframes.git
```

Or add it to `Packages/manifest.json`:

```json
"com.reromanlee.wireframes": "https://github.com/reromanlee/Wireframes.git"
```

## Quick start

```csharp
using reromanlee.Wireframes;
using UnityEngine;

public class TargetingLine : MonoBehaviour
{
    [SerializeField] private Transform _hand;
    [SerializeField] private Transform _target;

    private LineContainer _wireframes;

    private void Start()
    {
        _wireframes = new LineContainer();

        // A line from the hand to the target: create it where both are, then attach its ends.
        ILine line = _wireframes.CreateLine(_hand.position, _target.position);
        line.BoneA = _hand;
        line.BoneB = _target;
        line.SetColor(Color.cyan);

        // A box around the target that moves, rotates and scales with it.
        IBox box = _wireframes.CreateBox();
        box.Bone = _target;
        box.LocalCornerA = new Vector3(-0.5f, -0.5f, -0.5f);
        box.LocalCornerB = new Vector3(0.5f, 0.5f, 0.5f);
    }

    private void OnDestroy()
    {
        _wireframes.Dispose();
    }
}
```

There is no `Update`: both shapes follow `_hand` and `_target` on their own.

## Concepts

### Container

`new LineContainer()` creates a GameObject named **Wireframes** in the active scene, with one skinned mesh for all of its shapes.

- It lives until you call `Dispose()` or its scene unloads. Either way, every shape it created is disposed too, and `IsDisposed` becomes true.
- `new LineContainer(material)` draws with your material instead. The container never destroys a material you pass in.
- Create containers and shapes on the main thread, from `Awake`, `Start` or later, not from constructors or field initializers.

### Positions and bones

Every endpoint (and every box) has an optional **bone**: any Transform it follows. `null` means world space.

- `LocalPosition*` is relative to the bone. Without a bone it is a world position.
- `WorldPosition*` converts through the bone's current pose, like `Transform.position`.
- Changing a bone keeps the endpoint where it is in the world, like reparenting a Transform. From then on it moves with the new bone.

**Rule of thumb: set the bone first, then the position in whichever space you mean.**

| You want | Write |
|---|---|
| A line joining two transforms | `CreateLine(a.position, b.position)`, then `BoneA = a; BoneB = b;` |
| A point at an offset from a bone, e.g. a sword tip | `BoneB = sword; LocalPositionB = new Vector3(0f, 1f, 0f);` |
| A point pinned where something was hit, moving with it | `BoneA = hitTransform; WorldPositionA = hit.point;` |
| An endpoint that stops following | `BoneA = null;` (it stays where it is) |

### Boxes

A box is spanned by two opposite corners and is axis-aligned in its bone's space, so it moves, rotates and scales rigidly with that bone. To resize it, move its corners.

- A box has one bone. A box whose corners followed two independent bones would stop being a box as soon as they moved.
- Changing the bone keeps both corners' world positions and re-aligns the box to the new bone's axes.

### Colors

- Lines have a color per endpoint, blended along the line. `SetColor` sets both. Boxes have one color, set with `SetColor`.
- Colors are stored with 8 bits per channel, so values above 1 are clamped: no HDR glow.
- In linear color space (the Unity 6 default), the default shader converts vertex colors so they look the same as that `Color` on a material.
- The default material is opaque, so alpha has no effect.

### When edits show up

Setters only record the change. Each edited shape is queued once per frame, however many properties you set. The queue is uploaded at the end of `LateUpdate`, after scripts with the default execution order and before Unity renders, so edits appear in the same frame. Edits made later in the frame, such as in `WaitForEndOfFrame` or camera callbacks, appear in the next one.

### Destroyed bones

When a bone's GameObject is destroyed, the endpoints attached to it stay exactly where they were and switch to world space: their bone becomes `null`. The shapes themselves stay alive.

This works through a small hidden component that the package adds to every GameObject used as a bone and removes when no shape uses it anymore. If a bone's GameObject is destroyed without ever having been active, Unity doesn't notify that component. The package still notices, but the bone's last pose is lost, so its endpoints use their local offsets as world positions and a warning is logged.

### Disposing shapes

`shape.Dispose()` removes a shape and frees its space for the next shape of the same size. After that, `IsDisposed` is true and every other member throws `ObjectDisposedException`.

## Performance

- **One mesh, one draw call and one skinned renderer per container**, however many shapes and shape types it holds.
- **Moving bones costs the package nothing.** Unity computes one matrix per bone and skins the vertices on the GPU.
- **Edits cost only what changed.** Each edited shape is written once per frame, and only the changed parts of the GPU buffers are uploaded.
- **Adding and removing shapes is cheap on average.** Buffers double in size when full and never shrink. Freed space is reused by the next shape of the same size, and it is packed away once it fills half the buffer.
- **Memory:** 20 bytes per vertex on the GPU (position, color, bone index), plus a CPU copy. A line uses 2 vertices and 1 edge; a box uses 8 vertices and 12 edges. Indices start 16-bit and switch to 32-bit automatically once the vertex buffer outgrows them.

Measured by the package's performance test in the Editor on a desktop PC (Unity 6000.5, Direct3D 12), for 10,000 lines on 100 bones:

| Operation | CPU time |
|---|---|
| Create the lines and attach them | 4.2 ms |
| First upload | 1.9 ms |
| A frame where nothing changed | 0.1 µs |
| A frame where all 100 bones moved | 2.3 µs |
| 1,000 scattered color edits | 0.1 ms, plus 0.3 ms upload |
| Dispose 5,000 lines | 0.4 ms, plus 0.6 ms upload |

Two things to keep in mind:

- **GPU skinning must be on.** If a project disables it in Player Settings, or the platform has no compute shaders (for example WebGL), Unity skins on the CPU every frame, at a cost that grows with the vertex count.
- **Shapes are never frustum-culled.** Shapes follow arbitrary Transforms, so the mesh uses fixed bounds of ±1,000 km around the world origin instead of recomputing them every frame.

## Limitations

- Lines are always 1 pixel wide.
- The default material is opaque and depth-tested. For transparency or always-on-top lines, pass your own material.
- Main thread only. Shapes are drawn in Play mode and in builds, not in Edit mode.
- One bone per box.

## Samples

**Stress Test** spawns 10,000 lines and 1,000 boxes on 100 orbiting bones. Import it from the package's **Samples** tab in the Package Manager, add the `StressTest` component to an empty GameObject in a scene with a camera, and enter Play mode with the Profiler open. Raise **Recolor Per Frame** to measure the cost of edits.

## Tests

To run the package's tests in the Test Runner, add the package to `testables` in `Packages/manifest.json`:

```json
"testables": ["com.reromanlee.wireframes"]
```

## Roadmap

- Many more shape types (spheres, circles, arrows, frustums and others), all drawn by the same mesh.
- Text drawn with lines.
- Built-in transparent and always-on-top materials.

## License

[MIT](LICENSE.md)
