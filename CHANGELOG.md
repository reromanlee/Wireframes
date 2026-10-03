# Wireframes (Unreleased)

### What's new

1. Shape components draw a shape on their GameObject without code, in Edit Mode, Play Mode and builds: **Add Component > Wireframes** has one for every shape, such as `WireframeSphere` and `WireframeLine`.
2. Every change to a component shows up in the next render, made in the Inspector, by undo, animation or a script, and components do no work per frame.
3. Disabling a component or its GameObject hides its shape at no cost, and enabling it again allocates nothing.
4. Components share containers by occlusion, transparency and layer, so many of them draw in a few draw calls; a color with alpha below 1 draws transparent, and shapes draw on their GameObject's layer.
5. `DrawAsGizmo` draws a component's shape like a gizmo, in the Scene view and in the Game view while its Gizmos button is on, and builds leave it out.
6. Prefab Mode draws the components of the prefab being edited.
7. The Shape Gallery sample has a ComponentGallery scene made of components, and continuous integration builds it too.
8. A container without shapes skips its work before each render once its empty chunks are released.

### Known issues

1. The Hierarchy's Scene visibility toggles don't hide components' wireframes, and the Gizmos menu's per-component checkboxes don't affect gizmo shapes.
2. Clicking a component's wireframe in the Scene view doesn't select its GameObject.
3. A script that changes a GameObject's layer moves its components' shapes to that layer on their next change.
4. Stereo rendering for XR is untested.

# Wireframes 2.0.0

### What's new

1. Shapes follow their bones through the package's own shader skinning instead of a `SkinnedMeshRenderer`, which is faster on WebGL and no longer depends on the GPU Skinning player setting.
2. The package's material draws under the Built-in Render Pipeline, URP and HDRP, hides, shows or fades lines behind other geometry through the `Occlusion` setting, and draws transparent colors through `UseAlpha`.
3. Containers and shapes work in Edit Mode, drawing in the Scene and Game views without ever being saved into scenes.
4. Unity 2022.3 is supported, where 1.0 needed Unity 6.
5. `IsVisible` hides a shape or a whole container without disposing it, and without allocating.
6. `WireframeContainerSettings` sets a container's name, material, layer, persistence across scenes, reserved capacity, occlusion and transparency.
7. Containers split shapes into chunks of up to 65,535 vertices with 16-bit indices, hold any number of shapes, and give memory back as shapes are disposed.
8. `container.Statistics` reports shapes, vertices, edges, bones, chunks and memory without allocating, Profiler markers time each step, and Profiler counters add up every container when the `com.unity.profiling.core` package is installed.
9. Server builds, missing shaders, destroyed bones and failing shapes are handled without exceptions reaching Unity's rendering, and each problem is logged once with a `[Wireframes]` prefix.
10. Steady frames allocate nothing and edits upload only what they change, which tests enforce; continuous integration runs the tests on Unity 2022.3 and 6000.5 and builds for WebGL, Android, iOS, macOS and Linux.
11. The README is rewritten, with detailed Features, Performance, Usage and Migration pages in the `Documentation` folder.

### Breaking changes

1. `LineContainer` is now `WireframeContainer`, `ILineContainer` is removed, and a material is passed through `WireframeContainerSettings`.
2. A custom material's shader must move vertices with `WireframesSkin` from the package's `Wireframes.hlsl`.
3. `Segments` and `Sides` are now `SegmentCount` and `SideCount`, and the `segments`, `sides` and star `points` parameters are now `segmentCount`, `sideCount` and `pointCount`.
4. `LocalEnd` and `WorldEnd` of long shapes are now `LocalEndB` and `WorldEndB`, next to the new `LocalEndA` and `WorldEndA`.
5. Edits and bone moves are applied right before each camera renders, instead of at the end of `LateUpdate`.
6. Bones must be Transforms in a scene, and resolutions are capped at 1,024 segments or sides and 512 star points.
7. Calls from other threads throw `InvalidOperationException` in the Editor and development builds.

### Known issues

1. Stereo rendering for XR is untested.

# Wireframes 1.0.0

### What's new

1. New shapes: polylines (`IPolyline`, created as polylines, polygons or triangles), rectangles, rounded rectangles, circles, ellipses, stars, spheres, ellipsoids, spiked spheres, cylinders, cones, capsules, stadiums, frustums and pyramids.
2. `IRigidShape`, shared by every shape except lines and polylines: one bone, a local and world position and rotation, and a `Color`. Changing the bone keeps the shape's world position, rotation and size.
3. `IAxialShape`, shared by long shapes: `Length`, and end B through `LocalEnd` and `WorldEnd`.
4. Create methods that take bones first and work in their local space, such as `CreateLine(boneA, boneB)` and `CreateCircle(bone, localCenter, radius)`.
5. Polylines have a bone and a color for every point.
6. `WireframeCamera`, a component that draws everything its camera renders as wireframe, under the Built-in Render Pipeline or any Scriptable Render Pipeline.
7. The Shape Gallery sample, a scene with every shape, and a Stress Test sample that spawns every kind of shape.

### Changes

1. The package lives in the repository's `UnityPackage` folder; install it from `https://github.com/reromanlee/Wireframes.git?path=/UnityPackage`.
2. Boxes have a center, a rotation and a signed `Size`. Their corner properties still work, and `CreateBox(center, rotation, size)` creates a rotated box.
3. Changing a box's bone keeps its world rotation instead of aligning the box to the new bone's axes; `CreateBox(bone, localCornerA, localCornerB)` creates a box aligned to a bone.
4. `CreateBox()` creates a 1 by 1 by 1 cube instead of a box with both corners at the origin.

# Wireframes 0.1.0

### What's new

1. `LineContainer`, which draws all of its shapes with one GPU-skinned mesh. It is disposed by `Dispose()` or when the scene it was created in unloads, and accepts an optional material.
2. Lines (`ILine`) with local and world positions, colors and a bone for each endpoint.
3. Boxes (`IBox`) with local and world corners, one bone and `SetColor`.
4. `IShape`, shared by every shape: `SetColor`, `Dispose` and `IsDisposed`.
5. Shapes whose bone is destroyed stay in place and switch to world space.
6. Edits are applied once per frame and upload only what changed.
7. The Stress Test sample.
8. Edit Mode and Play Mode tests.
