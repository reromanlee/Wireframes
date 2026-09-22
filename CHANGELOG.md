# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
