# Project Kristal

## Introduction

(The name Kristal comes from @Krixtalx, who patiently teached me (@MarcosCobena) compute shaders while working on Typhoon (previously PointNet). Thank you José Antonio! :-) )

Working on shaders leveraged, again, the need for some sort of testing tooling for graphics. We have tried a few times to create something for visual testing in Evergine, with no success.

This approach is different:
- tests are thought to be created in the shared project, instead of a separate testing one
- tests are run along with the app (just before it, to be precise), instead of in a separate process
- tests take screenshots in the end, instead of asserting values

I am not sure whether will have time to work on it, so I have made it publicly available so others (you!) may contribute. Thank you in advance!

The goal? A public NuGet package (similar to Evergine.Mocks) which anyone can add to make source code directly related to graphics more maintanable.

## Current state

There are a bunch of TODOs through the source code with feedback/ideas on the next steps.
