# Release checklist

Continuous integration checks what a machine without a GPU can: the tests on the oldest and newest supported Unity, drawn by a software renderer, and player builds for every platform. The rest is checked by hand before each release, most important first.

### Before merging the release pull request

1. Every check of the pull request passes: the tests on both Unity versions, and the WebGL, Android, iOS, macOS and Linux builds.
2. The whole test suite passes locally on a real GPU, on the oldest and the newest supported Unity.
3. The Stress Test sample runs smoothly in Windows, Android and WebGL builds, with frame, main thread and GPU times no worse than the previous release's.
4. Both scenes of the Shape Gallery sample and all three occlusion modes look right on the same devices, in the Built-in, URP and HDRP pipelines.
5. In the Editor, under each pipeline, shape components draw in the Scene and Game views and in Prefab Mode, Draw As Gizmo shapes show only in the Scene view and in the Game view with Gizmos on, and clicking a wireframe never selects a hidden container.
6. A Profiler capture of an Android development build under load shows no new spikes in the `Wireframes.*` markers and no allocations in steady frames.
7. The timings in the test results artifacts are no worse than the previous release's.
8. `package.json` has the new version, `CHANGELOG.md` has its section, and the README and `Documentation/` describe the release; a major release also updates `Documentation/MIGRATION.md`.

### After merging

1. Tag the squashed commit on `main` with the version alone, such as `2.0.0`.
2. Export the signed tarball of the package from the Package Manager.
3. Draft the GitHub release from the tag, named like `v2.0.0 (signed)`, with the changelog section as its notes and the tarball attached.
