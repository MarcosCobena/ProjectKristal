# Tasks

## 1. Sustitución del paso fijo en el host

- [x] 1.1 Añadir a `ProjectKristal/MyApplication.cs` el override de `UpdateFrame(TimeSpan gameTime)` que, cuando `IsTestActive()` sea true, sustituye `gameTime` por `TimeSpan.FromSeconds(1.0/60.0)` y delega en `base.UpdateFrame(fixed)`; en caso contrario delega con el `gameTime` original. Orden alfabético de `using`. Verificar: compila; en una sesión normal con `MyScene` el delta que llega a la base sigue siendo el real.
- [x] 1.2 Añadir a `MyApplication` el predicado privado `IsTestActive()` que lee `screenContextManager.CurrentContext` (resuelto del container, como ya hace `Initialize`), devuelve `false` si es `null`, y en caso contrario recorre sus escenas por el indexador `ctx[i]` (i en 0..ctx.Count) devolviendo `true` si alguna es `SceneTest` (asignación por tipo para cubrir herencia `ModelTest : VisualTest : SceneTest`). Verificar: compila; `IsTestActive()` devuelve true mientras un test está activo y false cuando lo está `MyScene`.

## 2. Determinismo de la captura

- [x] 2.1 Ejecutar `ModelTest` en modo normal dos veces consecutivas, guardar las dos capturas `Screenshots/ModelTest.png` y comprobar con `ImageSharpCompare` (o comparación de hash de archivo) que son idénticas píxel a píxel entre sí. Verificar: dos archivos con el mismo hash.
- [x] 2.2 Confirmar que la captura de `ModelTest` coincide contra `ModelTest-expected.png` regenerada con el paso fijo aplicado (regenerar primero si hace falta con `-RegenerateScreenshots`, luego validar en modo normal). Verificar: `EqualImages()` devuelve true sin excepción.
- [x] 2.3 Ejecutar `BackgroundColorTest` en modo normal y verificar que sigue pasando su comparación (no depende de `dt`; no debe romperse por el cambio). Verificar: `EqualImages()` devuelve true.

## 3. Producción intacta

- [x] 3.1 Lanzar la aplicación en modo normal (sin flags) y, tras los tests, dejar que se cargue `MyScene`; verificar que su animación usa tiempo real (movimiento fluido, no jerárquico). Verificar: `MyScene` animada con el `gameTime` real sin reteo visible.
- [x] 3.2 Confirmar con logging puntual (o breakpoint) que, con `MyScene` activa, `UpdateFrame` delega con el `gameTime` real y no con el paso fijo. Verificar: el delta pasado a `base.UpdateFrame` es el del `Stopwatch`, no 1/60 s.

## 4. Documentación

- [x] 4.1 Añadir a `README.md` una nota breve que cierre el bucle: los tests con movimiento dependiente del tiempo corren con paso fijo de 60 FPS para que la captura sea determinista; el modo producción no se ve afectado. Verificar: la nota describe el comportamiento observado en 3.1.
