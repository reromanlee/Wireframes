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

Texts and symbols take what they draw last: `CreateText(text)`, `CreateText(position, rotation, text)` and `CreateText(bone, localPosition, localRotation, text)`, and the same three for `CreateSymbol` with a member of a symbol enum.

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
3. **Texts and symbols stand in their XY plane** and read from its -Z side, so with no rotation they face a camera looking along +Z.
4. Other shapes are centered on their position.
5. `Quaternion.LookRotation(axis, normal)` is the rotation that points a shape's +Z along `axis` with +Y toward `normal`.
6. A shape created from two points turns so that its +Y stays as close to world up, or its bone's up, as it can. A stadium or ellipse created that way lies as flat as it can.
7. Capsule and stadium ends are the centers of their spheres or circles, as in `Physics.CapsuleCast`. When one sphere holds the other, only the bigger one is drawn.

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

### Components

To draw a shape without writing code, add its component to a GameObject: **Add Component > Wireframes** has one for every shape, from **Line** to **Pyramid**. It draws there right away, in Edit Mode, Play Mode and builds, and follows the GameObject's Transform like a mesh, scale included. Its fields place the shape relative to the GameObject:

| Components | Place the shape with |
|---|---|
| `WireframeBox`, `WireframeRectangle`, `WireframeRoundedRectangle`, `WireframeCircle`, `WireframeEllipse`, `WireframeStar`, `WireframeSphere`, `WireframeEllipsoid`, `WireframeSpikedSphere` | **Center** and **Rotation**, in Euler angles. Flat shapes lie flat on the GameObject with no rotation. |
| `WireframeCylinder`, `WireframeCone`, `WireframeCapsule`, `WireframeStadium`, `WireframeFrustum`, `WireframePyramid` | **End A** and **End B**, the ends of the shape's axis, and **Roll** around it. A cone's or pyramid's tip is end A. With no roll, the shape's +Y stays as close to the GameObject's up as the axis allows. |
| `WireframeLine`, `WireframePolyline` | A **Bone** and a **Position** for each point: the point follows that Transform, or the GameObject when it has none. |
| `WireframeText`, `WireframeSymbol` | **Center** and **Rotation**, standing in the GameObject's XY plane and read from its -Z side. **Text**, **Glyphs** and the layout fields match [`IText`](#text), and **Symbol** lists the symbols of the component's glyphs by name. |

1. **Every change shows up in the next render,** made in the Inspector, by undo, a prefab revert, animation or a script. A count that runtime shapes fix at creation, such as **Segment Count**, creates the shape again, once you finish typing it, and invalid counts snap to the nearest valid one.
2. **`enabled` shows and hides the shape.** Disabling the component or its GameObject costs nothing, and enabling it again allocates nothing.
3. **One color per shape.** **Color** colors all of it, and an alpha below 1 draws it transparent; **Occlusion** works like the container setting. Shapes draw on their GameObject's layer, for camera culling masks.
4. **Draw As Gizmo** draws the shape like a gizmo: in the Scene view, and in the Game view only while its **Gizmos** button is on. Other cameras never draw it, and builds leave it out. To leave out a whole GameObject meant for debugging, tag it **EditorOnly**, which strips it from builds with its children.
5. **A destroyed bone counts as none,** so its point follows the GameObject from the next render on. A bone has to be in a scene: one that isn't, such as a prefab asset, is reported once and the GameObject followed instead.
6. **New components draw what the Create methods without arguments draw,** relative to the GameObject: a white shape of unit size, with long shapes running 1 unit along +Z. A line runs 1 unit forward, and a polyline starts as a small triangle; it draws nothing with fewer than 2 points, or 3 when closed. A text starts as "Text" and a symbol as the star, both drawn with the package's Default Glyphs.

Scripts set the same properties, and counts outside their range throw `ArgumentOutOfRangeException`, as Create methods do:

```csharp
WireframeLine line = gameObject.AddComponent<WireframeLine>();
line.BoneB = target;
line.PositionB = Vector3.up;
line.Color = Color.cyan;

WireframeSphere sphere = gameObject.AddComponent<WireframeSphere>();
sphere.Radius = 2f;
sphere.Occlusion = WireframeOcclusion.Show;
```

Components share containers, one per combination of occlusion, transparency, layer and **Draw As Gizmo**, so many components draw in a few draw calls. Those containers are never saved, stay out of the Hierarchy, and go away with their last component. Prefab Mode draws the components of the prefab being edited in its own scene.

1. The Hierarchy's Scene visibility toggles don't hide components' wireframes, and the per-component checkboxes of the **Gizmos** menu don't affect gizmo shapes.
2. A script that changes `gameObject.layer` moves the shape to that layer on the component's next change; a change in the Inspector moves it at once.
3. Clicking a wireframe in the Scene view doesn't select its GameObject.

### Text

`CreateText(position, rotation, text)` creates a text, and `CreateText(bone, localPosition, localRotation, text)` one that follows a bone. A text stands in the XY plane of its rotation and reads from the -Z side, so with no rotation it faces a camera looking along +Z.

```csharp
IText label = _wireframes.CreateText(_target, new Vector3(0f, 1.5f, 0f), Quaternion.identity, "Target");
label.CharacterSize = 0.5f;
label.SetColor(Color.cyan);
```

1. **Sizes are in glyph boxes.** `CharacterSize` is the height of a glyph box in the bone's units, 1 by default, and each line is one box tall. `CharacterSpacing` adds room after each character and `LineSpacing` between lines, both in boxes: 0.1 and 0 by default.
2. **`CharacterWidth` sets how far each character moves the next one along.** `Proportional`, the default, gives a character the width of its lines plus the spacing, and a space its pack's space width. `Monospace` gives every character a whole glyph box, so columns line up; with the Default Font, a `CharacterSpacing` of about -0.45 spaces it like a terminal.
3. **Lines are aligned in `Bounds`,** a rectangle centered on the text's position, 4 by 1 by default: each line by `HorizontalAlignment`, and all of them together by `VerticalAlignment`. With `Overflow` set to `Wrap`, a line wider than the bounds breaks at its last space that fits, or inside a word wider than the bounds; otherwise lines stay as written and run past the bounds.
4. **`\n` starts a new line,** `\r\n` counts once, a tab takes 4 spaces, and other control characters are skipped.
5. **A character the glyphs lack is drawn as `?`,** with one warning for each such character. `glyphs.Contains(character)` tells beforehand.
6. **`SetText` changes the text without allocating** once the text's buffers fit it, so a value can be rewritten every frame. Reading `Text` after `SetText` creates the string once.

```csharp
private readonly char[] _digits = new char[11];

private void Update()
{
    _score.TryFormat(_digits, out int length);
    _scoreText.SetText(_digits.AsSpan(0, length));
}
```

### Symbols

`CreateSymbol(position, rotation, symbol)` creates a symbol named by a member of the enum the Glyph Editor generates for a glyph pack, such as `DefaultSymbols.Heart`, and `CreateSymbol(bone, localPosition, localRotation, symbol)` one that follows a bone. Its glyph box is `Size` across, 1 by default, centered on its position, and stands in the XY plane of its rotation like a text.

```csharp
ISymbol marker = _wireframes.CreateSymbol(_target, new Vector3(0f, 2f, 0f), Quaternion.identity, DefaultSymbols.Warning);
marker.SetColor(Color.yellow);
marker.SetSymbol(DefaultSymbols.Check);
```

1. A symbol of the same name from any pack of its glyphs counts, so the enum of one pack can name a symbol that another pack draws instead.
2. `GetSymbol<TSymbol>()` returns the symbol as a member of any symbol enum, or as an undefined value when that enum has no member of its name.
3. The enum's `None` member draws nothing, and a symbol the glyphs lack is drawn as `?`, with one warning.

### Glyph packs

Texts and symbols draw **glyphs**: lines in a box from 0 to 1 on both axes, whose baseline is 0.25 up from the bottom. A **glyph pack**, `WireframeGlyphPack`, holds characters, found by their character, and symbols, found by their keyword. A **glyph list**, `WireframeGlyphs`, lists packs from the highest priority down: the first pack that has a character or symbol draws it, so a pack overrides the packs below it, and the list's Inspector says which glyphs each pack overrides.

1. **The package's Default Glyphs,** `WireframeGlyphs.Default`, list its Default Font, with the 95 printable ASCII characters, and its Default Symbols, with 23 symbols from `ArrowUp` to `Bolt`. Texts and symbols without glyphs of their own draw with them, and new components get them.
2. **To draw with glyphs of your own,** create a pack with **Create > Wireframes > Glyph Pack** and edit it in the Glyph Editor. Then create a list with **Create > Wireframes > Glyphs**, put your pack first and the package's packs below it, and set the list as the `Glyphs` of your texts, symbols and components.
3. **Code names symbols by the enum** that the Glyph Editor's **Generate Enum** writes next to the pack: a member per symbol, valued by its keyword's hash, plus `None`. Keywords are C# identifiers, and lookups compare hashes, never strings. Generate the enum again after adding, renaming or reordering symbols; the button stands out while the enum is out of date.
4. **Packs installed from a registry, git or a tarball are read-only,** as Unity keeps those packages. **Duplicate to Assets** in the Glyph Editor copies one into your project, where you can edit it.

### Glyph Editor

**Window > Wireframes > Glyph Editor** edits glyph packs; double-clicking a pack, or its Inspector's **Open in Glyph Editor** button, opens it there.

1. **The gallery** shows the pack's characters, sorted by character, and its symbols, in pack order or by name with **A–Z**, each named below its tile, and the search field filters both. **+** adds a character or a symbol, and **+ ASCII** adds an empty glyph for every printable ASCII character the pack lacks. Right-click a tile to rename, duplicate, copy, paste or delete its glyph, and drag symbols into another order, which is their enum's order.
2. **Clicking a tile opens its glyph** on the canvas beside the gallery. Click a point to select it, and drag it to move it, snapped to the grid unless Shift is held. Ctrl+click, or Cmd+click on macOS, adds a point after the end of the selected stroke, or starts a stroke when none is selected, and double-clicking a line inserts a point there. Delete removes the selected point, the arrow keys nudge it by a grid step, the wheel zooms, the middle button pans, F frames the box, and Esc closes the glyph.
3. **The details panel** lists each stroke's points as exact X and Y values, adds, reorders and deletes strokes and points, and closes a stroke back to its first point. It also centers the glyph horizontally or vertically: symbols suit both, while characters should stay on the baseline. Proportional text measures characters by their lines, so only monospace text cares where a character sits across its box.
4. **The preview strip** draws sample text with the pack alone, proportional or monospace, and lists the characters the pack lacks instead of warning about them.
5. **Pack Settings** sets the enum's name and namespace, the space width, the x-height and cap-height guides and the grid, and **Generate Enum** writes the enum.
6. Every edit can be undone, texts and symbols that draw with the pack show each edit at once, and Ctrl+S saves the pack.

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

1. **Shape Gallery** lays out the package's shapes in three rows, each turning with its own bone and named below it with text. Open its **ShapeGallery** scene and enter Play Mode. Its **ComponentGallery** scene has the same shapes made of components, drawn as soon as it opens, and turning in Play Mode. Its **TextAndSymbols** scene shows every default symbol, text in both character widths, wrapped and aligned, and a clock and a symbol drawn from code.
2. **Stress Test** spawns 10,000 lines, 1,000 boxes and 2,000 other shapes on 100 orbiting bones. Add the `StressTest` component to an empty GameObject in a scene with a camera, and enter Play Mode with the Profiler open. Raise **Recolor Per Frame** to measure the cost of edits.

### Tests

To run the package's tests in the Test Runner, add the package to `testables` in `Packages/manifest.json`:

```json
"testables": ["com.reromanlee.wireframes"]
```

The timing tests run when the project also has the `com.unity.test-framework.performance` package, and the Profiler counter test when it has `com.unity.profiling.core`.
