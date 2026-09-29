# Migrating from 1.x to 2.0

Wireframes 2.0 draws shapes through its own shader skinning instead of a `SkinnedMeshRenderer`, and renames the container. Most projects only need the renames; the other changes matter to projects with custom materials, or that depend on when edits are uploaded. Most important first.

### Renames

1. **`LineContainer` is now `WireframeContainer`,** and `ILineContainer` is gone. A container's options moved into `WireframeContainerSettings`:

   ```csharp
   // 1.x
   var wireframes = new LineContainer(material);

   // 2.0
   var wireframes = new WireframeContainer(new WireframeContainerSettings { Material = material });
   ```

2. **Resolution members end in `Count`:** `Segments` is now `SegmentCount` and `Sides` is `SideCount`, and the parameters `segments`, `sides` and the star's `points` are now `segmentCount`, `sideCount` and `pointCount`. Only calls that name those arguments change.
3. **Long shapes name both ends:** `LocalEnd` and `WorldEnd` are now `LocalEndB` and `WorldEndB`, next to the new `LocalEndA` and `WorldEndA`.

Create methods became extension methods, declared in a factory class next to each shape, such as `CircleFactory`, so calls like `container.CreateCircle(...)` stay as they are with `using reromanlee.Wireframes;`.

### Custom materials

In 1.x, a `SkinnedMeshRenderer` moved the vertices, so any vertex color shader worked. In 2.0 the shader moves them: a custom material's shader has to call `WireframesSkin` from the package's include file, or shapes are drawn in their bones' local space around the world origin. [USAGE.md](USAGE.md#custom-materials) shows the vertex program.

The package's material now works under URP and HDRP too, and draws transparent or always-on-top lines through the `UseAlpha` and `Occlusion` settings, so a custom material made only for those reasons can go.

### When edits show up

1.x uploaded edits at the end of `LateUpdate`. 2.0 uploads them, and reads the bones, right before each camera renders, so edits made after `LateUpdate` but before rendering now show up in the same frame. Edits made after rendering, such as in `WaitForEndOfFrame`, still show up in the next one.

### Bones

1. No component is added to bones anymore. A destroyed bone is noticed when the bones are read, and its shapes stay where they were last drawn, switching to world space the next time they are used.
2. A bone must be a Transform in a scene: a prefab asset throws `ArgumentException`.

### Limits and checks

1. Resolutions are capped at 1,024 segments or sides and 512 star points, and larger values throw `ArgumentOutOfRangeException`.
2. Calls from other threads, never supported, now throw `InvalidOperationException` in the Editor and development builds.

### What changes around the container

1. A container draws with a `MeshRenderer` per chunk of up to 65,535 vertices, instead of one `SkinnedMeshRenderer`.
2. The **GPU Skinning** player setting no longer matters, and WebGL no longer skins shapes on the CPU.
3. Unity 2022.3 is supported, where 1.x needed Unity 6.
4. Containers also work in Edit Mode, never saved into scenes.
