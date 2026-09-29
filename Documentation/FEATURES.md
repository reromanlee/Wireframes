# Features

Everything Wireframes does, most important first. How to use each part is in [USAGE.md](USAGE.md).

### Shapes follow Transforms on the GPU

A shape attaches to Transforms, called bones, and moves, turns and scales with them from then on. Once per frame, right before a camera renders, the package reads each bone's local-to-world matrix and uploads it to a float texture; the vertex shader moves every vertex by its bone's matrix.

1. A bone costs the same whether one shape or a thousand follow it.
2. Lines and polylines have a bone per point, so they can join anything; other shapes have one bone and move rigidly with it, like a child Transform.
3. Changing a shape's bone keeps it where it is in the world, like reparenting a Transform.
4. When a bone is destroyed, its shapes stay where they were drawn last and switch to world space.
5. Any Transform in a scene can be a bone. Nothing is added to it, and a prefab asset is refused with an `ArgumentException`.

### 17 shapes

| Shape | Usual Create method | What it is |
|---|---|---|
| `ILine` | `CreateLine(a, b)` | A segment; each end has its own bone. |
| `IPolyline` | `CreatePolyline(points)`, `CreatePolygon(points)`, `CreateTriangle(a, b, c)` | Points joined in order, open or closed; each point has its own bone and color. |
| `IBox` | `CreateBox(cornerA, cornerB)` | A box with a center, a rotation and a `Size`. |
| `IRectangle` | `CreateRectangle(cornerA, cornerB)` | A flat rectangle. |
| `IRoundedRectangle` | `CreateRoundedRectangle(cornerA, cornerB, cornerRadius)` | A flat rectangle with round corners. |
| `ICircle` | `CreateCircle(center, radius)` | A flat circle; `CreateCircle(center, normal, radius)` faces a direction. |
| `IEllipse` | `CreateEllipse(tipA, tipB, radius)` | A flat ellipse whose long axis runs between two tips. |
| `IStar` | `CreateStar(center, innerRadius, outerRadius, pointCount)` | A flat star. |
| `ISphere` | `CreateSphere(center, radius)` | Three great circles. |
| `IEllipsoid` | `CreateEllipsoid(tipA, tipB, radius)` | Three ellipses, with a radius along each axis. |
| `ISpikedSphere` | `CreateSpikedSphere(center, baseRadius, spikeLength, spikeCount)` | A 3D star: a Platonic solid with a spike on each face, so 4, 6, 8, 12 or 20 spikes. |
| `ICylinder` | `CreateCylinder(endA, endB, radius)` | Two rings joined by four lines. |
| `ICone` | `CreateCone(tip, baseCenter, radius)` | A base ring joined to the tip by four lines. |
| `ICapsule` | `CreateCapsule(centerA, centerB, radius)` | Two spheres wrapped together, like `Physics.CapsuleCast`; each end can have its own radius. |
| `IStadium` | `CreateStadium(centerA, centerB, radius)` | The flat outline of a capsule. |
| `IFrustum` | `CreateFrustum(endA, endB, radiusA, radiusB, sideCount)` | Two regular polygons joined at every corner. Prisms and regular pyramids are frustums too. |
| `IPyramid` | `CreatePyramid(tip, baseCenter, baseSize)` | A rectangular base joined to a tip, like a camera's view without its near plane. |

Every shape except lines and polylines also has a Create method on a bone, in its local space, one from a position and a rotation, and one without arguments for a white shape of unit size. Round shapes are drawn like Unity's gizmos, with rings and a few lines, from 3 to 1,024 segments per ring.

### Edits upload only what they change

A setter only records the change, and each edited shape is written once per frame however many of its properties changed. Only the changed ranges of the GPU buffers are uploaded, and moving bones uploads nothing but their matrices. Tests check that editing one shape among many uploads that shape alone.

### Steady frames allocate nothing

Frames that move bones, edit shapes, hide and show them, or change nothing allocate no managed memory, which tests enforce. Freed space is reused by the next shape of the same size and packed away once it fills half a buffer, and memory past the reserved capacity is given back as shapes are disposed.

### Containers

A `WireframeContainer` creates and draws shapes, and disposes them when it is disposed or when the scene it was created in unloads. `WireframeContainerSettings` chooses how, and is serializable, so a script can show it in the Inspector.

| Setting | Default | What it does |
|---|---|---|
| `Occlusion` | `Hide` | What is drawn of lines that other geometry hides: nothing, everything, or a dimmer line. |
| `UseAlpha` | false | Blends each color by its alpha instead of drawing opaque lines. |
| `Material` | none | Draws with your material instead of the package's. |
| `Layer` | 0 | Layer of the container's GameObjects, for camera culling masks. |
| `PersistAcrossScenes` | false | Keeps the container when other scenes load. |
| `VertexCapacity`, `EdgeCapacity` | 0 | Room reserved up front, so a known load never grows the buffers. |
| `Name` | "Wireframes" | Name of the container's GameObject. |

Shapes are stored in chunks, meshes of up to 65,535 vertices with 16-bit indices, created as they fill; a shape bigger than a chunk gets a mesh of its own with 32-bit indices. An unreserved chunk that stays empty for 5 seconds is released.

### Render pipelines and platforms

1. One shader draws under the Built-in Render Pipeline, URP and HDRP, picked by the active pipeline.
2. Windows, Android and WebGL are tested on devices before each release; macOS, iOS and Linux are built by continuous integration.
3. Without a graphics device, as in server builds, or without a usable shader, shapes keep working and nothing is drawn or uploaded.
4. A custom material works once its shader skins vertices with the package's `Wireframes.hlsl`.
5. The shader carries Unity's stereo rendering macros for XR, which is untested.

### Occlusion and transparency

`WireframeOcclusion.Hide` depth-tests lines like any other object, `Show` draws them over everything, and `Fade` draws their hidden parts dimmer, like the Scene view's handles, at the cost of a second draw call. `UseAlpha` blends every color by its alpha, in any occlusion mode.

### Play Mode, Edit Mode and builds

Containers and shapes work the same in Edit Mode, drawing in the Scene and Game views. There, a container is never saved into its scene and never marks it as changed; it is disposed before scripts reload and when its scene closes, unless it persists across scenes. Switching between Edit and Play Mode alone never disposes a container.

### Visibility

`shape.IsVisible = false` hides a shape without disposing it: it keeps its place, bones and settings, and draws again as it is then once shown. `container.IsVisible = false` hides every shape of a container and costs nothing while it lasts. Neither allocates.

### Statistics and profiling

1. `container.Statistics` reports shapes, hidden shapes, vertices, edges, bones, chunks, and CPU and GPU memory, without allocating.
2. Profiler markers named `Wireframes.*` time each step: reading bones, uploading them, writing shapes, uploading meshes, compacting, resizing buffers, and creating and releasing chunks.
3. With the `com.unity.profiling.core` package installed, Profiler counters add up every container's shapes, vertices, edges, bones, chunks and GPU memory.

### Reliability

1. A Create call that fails leaves nothing behind.
2. A shape that fails to update is reported once and skipped, and the others keep updating.
3. No exception escapes into Unity's rendering.
4. Warnings and errors start with `[Wireframes]` and are logged once per cause.
5. In the Editor and development builds, calls from other threads throw and positions that aren't finite numbers are reported; release builds skip both checks.
6. Everything works with domain reload disabled.

### Wireframe camera

The `WireframeCamera` component draws everything its camera renders as wireframe, like the Scene view's Wireframe draw mode, while other cameras draw normally. It works under every render pipeline, and needs a graphics API with wireframe rendering, which OpenGL ES and WebGL lack.
