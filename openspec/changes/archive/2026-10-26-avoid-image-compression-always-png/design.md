# Design

## Context

Ver `proposal.md - Why` para la motivación. Estado actual relevante:

- `Evergine.VisualTests/VisualTest.cs` escribe la captura con `Camera3D!.FrameBuffer.ColorTargets[0].Texture.SaveToFile(GraphicsContext, path)`. Verificado en runtime: los ficheros resultantes (`BackgroundColorTest-expected.png`, `ModelTest-expected.png`) llevan cabecera `JFIF` (JPEG), no `\x89PNG`, pese a la extensión `.png`. El codec lo decide el motor por contenido, no por la extensión del fichero.
- `EqualImages()` compara vía `Codeuctivity.ImageSharpCompare ImagesAreEqual(actualFilePath, expectedFilePath)`, descodificando ambos `.png` (en realidad JPEG) desde disco. La comparación tolera hoy el error sistemático del JPEG solo porque ambas imágenes pasan por el mismo codificador.
- El framebuffer de la cámara se crea con `PixelFormat.R8G8B8A8_UNorm` (en `Program.cs` para el swapchain y en `SceneTest.CreateScene` por `CreateFrameBuffer(1280, 720)`), de modo que el readback produce bytes RGBA8 packed — sin necesidad de reordenar canales.
- La API pública de Evergine `2026.5.26.2553` (versión que el proyecto restaura) expone, sin reflexión: `GraphicsContext.Factory.CreateTexture(TextureDescription, debugName)`, `TextureDescription` con `Usage=Staging`/`ResourceCpuAccess.Read`, `GraphicsContext.Factory.CreateCommandQueue(CommandQueueType.Graphics)`, `CommandQueue.CommandBuffer()` con `Begin()`/`End()`, `CommandBuffer.CopyTextureDataTo(source, destination)`, `CommandQueue.Submit()`/`WaitIdle()`, `GraphicsContext.MapMemory(resource, MapMode.Read, subresource)` → `MappedResource { Data, RowPitch, SizeInBytes }`, y `GraphicsContext.UnmapMemory(resource, subresource)`.

## Goals / Non-Goals

**Goals:**
- Que toda captura se escriba como PNG real, sin compresión con pérdida.
- Que el readback GPU→CPU use exclusivamente la API pública de Evergine, sin reflexión.
- Que la comparación opere sobre bytes RGBA en memoria, con tolerancia exacta (cero), eliminando la dependencia externa de comparación de imágenes.

**Non-Goals:**
- Cambiar el paso de simulación fijo (1/60 s) de los tests: su determinismo queda fuera de este cambio.
- Mover el helper de captura al host (`ProjectKristal.Windows`): vive en la lib local `Evergine.VisualTests`, coherente con su aspiración a paquete público (ver README).
- Adoptar el paquete NuGet oficial `Evergine.VisualTests` recién publicado: se mantiene la solución autocontenida con SkiaSharp, según decisión del usuario.
- Cambiar el flujo de regeneración (`-RegenerateScreenshots`, `AssemblyMetadata`, `frame==3`, `Pop`): se conserva íntegro del cambio anterior.

## Decisions

### Decisión 1: readback vía staging + MapMemory (sin reflexión)

**Elección:** crear una textura staging con `TextureDescription { Usage=Staging, CpuAccess=Read, Width/Height/Format iguales a la fuente }`, copiar la textura fuente a ella con `CommandBuffer.CopyTextureDataTo`, sincronizar con `CommandQueue.Submit()` + `WaitIdle()`, y leer los píxeles con `GraphicsContext.MapMemory(staging, MapMode.Read, 0)` → `MappedResource.Data` respetando `RowPitch` (los píxeles pueden no estar packed fila a fila), y `UnmapMemory`.

**Por qué no `MapMemory` directo sobre la textura fuente:** la textura del color target del framebuffer se crea con `ResourceUsage.Default` y `ResourceCpuAccess.None`, así que el motor no permite mapearla a CPU directamente. El patrón estándar de readback requiere una staging intermedia con acceso CPU de lectura.

**Por qué `WaitIdle()` y no asumir sincronía implícita:** el readback GPU→CPU es asíncrono por diseño; sin esperar, `MapMemory` podría devolver píxeles de un estado parcial o anterior. `CommandQueue.WaitIdle()` garantiza que la copia terminó antes del mapeo. Esto resuelve además la Open Question que el cambio anterior dejó abierta sobre la sincronía de `SaveToFile`.

**Alternativas descartadas:**
- Reflexión sobre miembros internos del motor para forzar PNG en `SaveToFile`: descartada por requisito explícito del usuario.
- Usar `TextureCapture.Save` del paquete NuGet oficial `Evergine.VisualTests`: descartada por decisión del usuario (solución autocontenida).

### Decisión 2: SkiaSharp como codec PNG

**Elección:** volcar los bytes RGBA8 packed a un `SKBitmap` con `SKColorType.Rgba8888` y `SKAlphaType.Unpremul`, y codificar con `SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream)`.

**Por qué sobre `System.Drawing.Common`:** SkiaSharp es multiplataforma y activamente mantenido; `System.Drawing.Common` solo se soporta en Windows a partir de .NET 6 (y el proyecto ya es `net10.0-windows`, pero Skia evita atar la captura a Windows de cara al futuro de la lib como paquete público).

**Por qué sobre SixLabors.ImageSharp:** el usuario excluyó explícitamente ImageSharp. Skia es la alternativa elegida.

### Decisión 3: comparación sobre bytes RGBA en memoria, tolerancia exacta

**Elección:** la referencia se carga a bytes RGBA una sola vez (leyendo el `-expected.png` con Skia), la captura ya está en bytes RGBA tras el readback, y se comparan canal a canal con tolerancia cero.

**Por qué tolerancia cero:** con PNG sin compresión con pérdida y paso fijo determinista, los bytes entre ejecuciones son idénticos. Cualquier tolerancia > 0 enmascararía regresiones reales (un cambio de color de 1 canal sería indetectable).

**Por qué eliminar `Codeuctivity.ImageSharpCompare`:** ya no se descodifican ficheros desde disco para comparar; la captura viva en memoria se compara directamente contra la referencia (también en memoria). Se elimina la dependencia externa y su arrastre de `SixLabors.ImageSharp`.

**Alternativa descartada:** mantener `ImageSharpCompare` comparando los nuevos PNG reales: funcionaría, pero dejaría una dependencia que ya no aporta valor y que el cambio anterior marcó como Non-Goal tocar.

### Decisión 4: el helper vive en `Evergine.VisualTests` (lib local)

**Elección:** `PngSaver` (readback + escritura PNG) y `PixelComparison` (comparación de bytes RGBA) se añaden a la lib local `Evergine.VisualTests`, y `SkiaSharp` se añade como `PackageReference` de esa lib.

**Por qué:** la lib aspira a paquete público (ver README); la captura y la comparación son responsabilidades del marco de tests, no del host. Mantenerlas en la lib deja al host (`ProjectKristal.Windows`) sin acoplar al codec.

## Risks / Trade-offs

- **`RowPitch` puede no igualar `width * 4`.** → La copia de la staging a memoria se hace fila a fila usando `MappedResource.RowPitch`, no como un único `memcpy`. Evita leer basura del padding.
- **El formato del framebuffer podría no ser RGBA8 en el futuro.** → Si cambiara a BGRA8, habría que aplicar `ImageHelpers.FromBGRA32ToRGBA32` antes de codificar/comparar. Hoy no aplica (es `R8G8B8A8_UNorm`); se deja documentado, no se implementa preventivamente.
- **SkiaSharp añade una dependencia nativa.** → SkiaSharp trae binaries nativos; `dotnet restore` los resuelve para Windows. No afecta al host más allá de la cadena de referencias lib→host.
- **Las `-expected.png` actuales son JPEG y deben regenerarse.** → Tras implementar, se ejecuta una vez en modo regenerar para producir los nuevos PNG reales y se commitean. Hasta entonces, la comparación fallaría (PNG vs JPEG).

## Migration Plan

1. Añadir `PackageReference` a `SkiaSharp` en `Evergine.VisualTests.csproj`; eliminar `Codeuctivity.ImageSharpCompare`.
2. Implementar `PngSaver` (readback staging + MapMemory + Skia encode) y `PixelComparison` (comparación de bytes RGBA, tolerancia cero).
3. Reescribir `VisualTest.SaveCapture()` para que llame a `PngSaver`, y `EqualImages()` para que use `PixelComparison`.
4. Ejecutar el launcher en modo regenerar y commitear los nuevos `-expected.png` como PNG reales.
5. Validar modo normal: las comparaciones pasan con las nuevas bases.

Rollback: revertir restaura `Texture.SaveToFile` + `ImageSharpCompare`; los `-expected.png` JPEG antiguos siguen siendo comparables entre sí (recuperarlos de git si se hubiesen sobrescrito).

## Open Questions

- Ninguna que cambie specs, enfoque o tareas.
