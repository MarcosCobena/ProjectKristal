# Proposal

## Why

Las capturas de los tests visuales se escriben hoy como JPEG aunque la extensión del fichero sea `.png`: el codec lo decide Evergine por contenido de la textura, no por la extensión, y los bytes resultantes llevan cabecera `JFIF`. Eso viola el contrato de "captura reproducible píxel a píxel" que la spec `visual-tests` ya exige: la comparación solo sobrevive porque tanto la captura actual como la esperada pasan por el mismo codificador JPEG, pero el error sistemático es frágil y opaco. El cambio anterior (`regenerate-screenshots-from-launcher`) dejó explícitamente como Non-Goal "cambiar el formato de imagen o la librería de comparación" y como Open Question la sincronía del `SaveToFile`; este cambio cierra ambos huecos.

## What Changes

- La captura de un test visual se escribe **siempre como PNG real, sin compresión con pérdida**. Los ficheros resultantes llevan cabecera `\x89PNG`, no `JFIF`.
- La captura se obtiene leyendo los píxeles de la textura de la GPU a la CPU usando **solo la API pública de Evergine** (`ResourceFactory.CreateTexture` con `Usage=Staging`/`CpuAccess=Read`, `CommandBuffer.CopyTextureDataTo`, `CommandQueue.Submit`/`WaitIdle`, `GraphicsContext.MapMemory`/`UnmapMemory`), **sin reflexión** sobre Evergine.
- El codec PNG pasa a ser **SkiaSharp** (`SKBitmap` + `SKImage.Encode(SKEncodedImageFormat.Png, ...)`), reemplazando `Texture.SaveToFile`.
- La comparación de imágenes deja de descodificar ficheros desde disco y pasa a operar **sobre los bytes RGBA obtenidos directamente del readback**, comparados píxel a píxel con **tolerancia exacta (0)** por canal. La dependencia `Codeuctivity.ImageSharpCompare` se elimina.
- Las capturas de referencia `-expected.png` existentes se regeneran como PNG real y se commitean, sustituyendo a los actuales con bytes `JFIF`.

## Capabilities

### New Capabilities
<!-- Ninguna: se modifica la capability existente visual-tests. -->

### Modified Capabilities
- `visual-tests`: la captura pasa a ser PNG sin compresión con pérdida obtenida por readback GPU→CPU con API pública de Evergine, y la comparación pasa a operar sobre los bytes RGBA directamente (tolerancia exacta), eliminando la dependencia externa de comparación de imágenes.

## Impact

- `Evergine.VisualTests/VisualTest.cs`: `SaveCapture()` y `EqualImages()` se reescriben sobre el nuevo helper de readback y comparación; desaparece la llamada a `Texture.SaveToFile`.
- `Evergine.VisualTests/` (nuevo): helper de captura PNG (`PngSaver`) y comparación de píxeles (`PixelComparison`) sobre bytes RGBA, con SkiaSharp como codec.
- `Evergine.VisualTests/Evergine.VisualTests.csproj`: se añade `PackageReference` a `SkiaSharp` y se elimina `Codeuctivity.ImageSharpCompare`.
- `ProjectKristal.Windows/Screenshots/*-expected.png`: se reemplazan los ficheros actuales (bytes `JFIF`) por PNG reales.
- `README.md`: si menciona el formato de las capturas o la librería de comparación, se actualiza.
