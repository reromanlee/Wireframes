# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-09-24

### Added

- Shapes: polylines (`IPolyline`, created as polylines, polygons or triangles), rectangles, rounded rectangles,
  circles, ellipses, stars, spheres, ellipsoids, spiked spheres, cylinders, cones, capsules, stadiums, frustums and
  pyramids.
- `IRigidShape`, shared by every shape except lines and polylines: one bone, a local and world position and rotation,
  and a `Color`. Changing the bone keeps the shape's world position, rotation and size.
- `IAxialShape`, shared by long shapes: `Length`, and end B through `LocalEnd` and `WorldEnd`.
- Create methods that take bones first and work in their local space, such as `CreateLine(boneA, boneB)` and
  `CreateCircle(bone, localCenter, radius)`.
- Polylines have a bone and a color for every point.
- Shape Gallery sample, a scene with every shape. The Stress Test sample spawns every kind of shape.

### Changed

- Boxes have a center, a rotation and a signed `Size`. Their corner properties still work, and
  `CreateBox(center, rotation, size)` creates a rotated box.
- Changing a box's bone keeps its world rotation instead of aligning the box to the new bone's axes.
  `CreateBox(bone, localCornerA, localCornerB)` creates a box aligned to a bone.
- `CreateBox()` creates a 1 by 1 by 1 cube instead of a box with both corners at the origin.

## [0.1.0] - 2026-09-22

### Added

- `LineContainer`, which draws all of its shapes with one GPU-skinned mesh. It is disposed by `Dispose()` or when
  the scene it was created in unloads, and accepts an optional material.
- Lines (`ILine`) with local and world positions, colors and a bone for each endpoint.
- Boxes (`IBox`) with local and world corners, one bone and `SetColor`.
- `IShape`, shared by every shape: `SetColor`, `Dispose` and `IsDisposed`.
- Shapes whose bone is destroyed stay in place and switch to world space.
- Edits are applied once per frame and upload only what changed.
- Stress Test sample.
- Edit mode and play mode tests.
