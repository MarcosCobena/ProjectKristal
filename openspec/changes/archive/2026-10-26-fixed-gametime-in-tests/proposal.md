# Proposal

## Why

`ModelTest` es el único test visual con movimiento animado (un `Spinner` que rota según el `gameTime`). Ese `gameTime` no es fijo: se origina en un `Stopwatch` de tiempo real medido en cada iteración del render loop, por lo que el ángulo acumulado del modelo al fotograma de captura varía entre ejecuciones. El resultado es que la captura real no coincide con la referenci­a y `EqualImages()` falla de forma no determinista. Los tests visuales pierden así su propiedad definitoria: la reproducibilidad píxel a píxel.

## What Changes

- El `gameTime` que llega a los tests se fija a un paso constante de 60 FPS (`1/60` s por fotograma) **solo cuando la escena activa es un test** (`SceneTest`).
- El punto de sustitución es `Application.UpdateFrame(TimeSpan)`, aguas arriba de `Scene.BaseUpdate` (que copia el delta al campo `sceneTime` que el `Spinner` recibe realmente). Sobrescribir el delta dentro del propio `Update` del test llegaría tarde.
- La sustitución está guardada por el tipo de la escena activa: cuando el contexto activo es `MyScene` (producción) o cualquier no-`SceneTest`, el `gameTime` real se conserva sin cambios.
- El comportamiento del modo regeneración se mantiene; la única diferencia es que ahora cada fotograma avanza un delta fijo, de modo que regenerar y validar producen el mismo fotograma determinista.

## Capabilities

### New Capabilities
<!-- Ninguna: este cambio refina una capability existente. -->

### Modified Capabilities
- `visual-tests`: el requisito del ciclo del test pasa de garantizar "capturar en un momento determinado" a garantizar que ese momento y el estado de la escena en él son **idénticos entre ejecuciones**, mediante un paso de simulación fijo. Se añade además el requisito de que el paso fijo solo aplica bajo test (no en producción).

## Impact

- `ProjectKristal/MyApplication.cs`: sobrescribe `Application.UpdateFrame(TimeSpan gameTime)`; cuando la escena activa del `ScreenContextManager.CurrentContext` sea un `SceneTest`, sustituye `gameTime` por `TimeSpan.FromSeconds(1.0/60.0)` antes de delegar a la base. Sin esa condición, el delta real se conserva.
- `Evergine.VisualTests/SceneTest.cs`: sin cambios de comportamiento, pero pasa a depender de que el paso fijo le llegue desde el host (documentación implícita del contrato determinista).
- `ProjectKristal.Windows/Program.cs`: sin cambios; el `Stopwatch` real sigue midiendo, y `MyApplication` lo sustituye solo bajo test.
- No introduce dependencias nuevas; se apoya en `Evergine.Framework` ya referenciado (`Application.UpdateFrame` y `ScreenContextManager` son `public`/`virtual`).
