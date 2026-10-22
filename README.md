# Project Kristal

## Introduction

(The name Kristal comes from @Krixtalx, who patiently teached me (@MarcosCobena) compute shaders while working on Typhoon (previously PointNet). Thank you José Antonio! :-) )

Working on shaders leveraged, again, the need for some sort of testing tooling for graphics. We have tried a few times to create something for visual testing in Evergine, with no success.

This approach is different:
- tests are thought to be created in the shared project, instead of a separate testing one
- tests are run along with the app (just before it, to be precise), instead of in a separate process
- tests take screenshots in the end, although can assert values as well (through xUnit's assertion library)

I am not sure whether will have time to work on it, so I have made it publicly available so others (you!) may contribute. Thank you in advance!

The goal? A public NuGet package (similar to Evergine.Mocks) which anyone can add to make source code directly related to graphics more maintanable.

## Visual tests

The test framework exposes three base classes:

- `SceneTest`: base of all tests. Sets up a 1280x720 camera, lets each test build its scene with `ArrangeActScene(Entity)`, and at frame 2 runs the `SaveCapture()` → `Assert()` → `Pop()` cycle.
- `VisualTest`: a test that compares an image capture against a reference (`-expected.png`). Use this for appearance checks; `Assert()` calls `EqualImages()`.
- `NonVisualTest`: a test that validates data or compute without capturing or comparing images.

### Running tests

In `Debug` builds, the launcher pushes every test before loading `MyScene`, so visual and non-visual tests run inline every time you launch the app. `VisualTest`s write `{ClassName}.png` next to the launcher binary and compare it against `{ClassName}-expected.png` (copied from `ProjectKristal.Windows/Screenshots/` at build time). `NonVisualTest`s neither write nor compare.

### Regenerating reference screenshots

When a `VisualTest`'s expected output changes, regenerate the reference images from the launcher itself:

```
dotnet build ProjectKristal.Windows/ProjectKristal.Windows.csproj
ProjectKristal.Windows/bin/Debug/net10.0-windows/ProjectKristal.Windows.exe -RegenerateScreenshots
```

In this mode the launcher:

1. Runs only `VisualTest`s (non-visual tests are skipped).
2. Writes each `{ClassName}-expected.png` directly into `ProjectKristal.Windows/Screenshots/` (the source folder tracked in git).
3. Exits after the last test, without loading `MyScene`.

Commit the updated `-expected.png` files. A subsequent normal launch compares against the new bases.

## Current state

There are a bunch of TODOs through the source code with feedback/ideas on the next steps.
