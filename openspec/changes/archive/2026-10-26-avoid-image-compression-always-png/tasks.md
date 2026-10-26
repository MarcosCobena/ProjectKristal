# Tasks

## 1. Dependencias

- [x] 1.1 Añadir `<PackageReference Include="SkiaSharp" Version="..." />` a `Evergine.VisualTests/Evergine.VisualTests.csproj`. Verificar: `dotnet restore` de la lib resuelve SkiaSharp y la build posterior encuentra los ensamblados de Skia.
- [x] 1.2 Eliminar `<PackageReference Include="Codeuctivity.ImageSharpCompare" ... />` de `Evergine.VisualTests/Evergine.VisualTests.csproj`. Verificar: `dotnet restore` ya no trae `Codeuctivity.ImageSharpCompare` ni `SixLabors.ImageSharp` en `project.assets.json`.

## 2. Helper de captura PNG

- [x] 2.1 Crear `PngSaver` en `Evergine.VisualTests/` que, dada una `GraphicsContext`, una `Texture` (color target) y una ruta, cree una textura staging (`Usage=Staging`, `CpuAccess=Read`, mismo `Width`/`Height`/`Format` que la fuente), copie la fuente a la staging con `CommandBuffer.CopyTextureDataTo`, sincronice con `CommandQueue.Submit()` + `WaitIdle()`, lea los píxeles con `MapMemory(MapMode.Read)` respetando `RowPitch`, y libere con `UnmapMemory`. Verificar: una llamada de prueba devolvería un `byte[]` RGBA8 del tamaño `width*height*4` sin excepciones.
- [x] 2.2 En `PngSaver`, volcar los bytes RGBA8 a un `SKBitmap` (`SKColorType.Rgba8888`, `SKAlphaType.Unpremul`) y codificar a PNG con `SKImage.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream)` escribiendo el fichero en la ruta dada. Verificar: el fichero resultante lleva la firma `\x89PNG` en sus primeros bytes y no contiene `JFIF`.

## 3. Comparación de píxeles

- [x] 3.1 Crear `PixelComparison` en `Evergine.VisualTests/` que, dados dos `byte[]` RGBA y dimensiones, compare canal a canal con tolerancia cero devolviendo un booleano. Verificar: dos vectores idénticos devuelven `true`; un byte distinto en cualquier canal devuelve `false`.
- [x] 3.2 Añadir a `PixelComparison` una carga de la referencia desde un fichero `.png` a `byte[]` RGBA mediante Skia (`SKBitmap.Decode` → bytes RGBA), para comparar la captura en memoria contra la referencia en disco. Verificar: al decodificar un `-expected.png` real, se obtiene un `byte[]` RGBA8 del tamaño esperado.

## 4. Integración en `VisualTest`

- [x] 4.1 Reescribir `VisualTest.SaveCapture()` para que llame a `PngSaver` en lugar de `Texture.SaveToFile`, tanto en modo normal (`{ClassName}.png`) como en modo regenerar (`{ClassName}-expected.png` en `ScreenshotsSourceDir`). Eliminar toda referencia a `SaveToFile`. Verificar: compila sin referencia a `SaveToFile`; en runtime el `.png` escrito lleva firma `\x89PNG`.
- [x] 4.2 Reescribir `VisualTest.EqualImages()` para que cargue la referencia a `byte[]` RGBA con `PixelComparison` y la compare contra los bytes RGBA de la captura actual (también en memoria), en lugar de `ImageSharpCompare.ImagesAreEqual`. Verificar: compila sin `ImageSharpCompare`; `Assert()` de `BackgroundColorTest` y `ModelTest` pasan en modo normal.

## 5. Regeneración y validación integral

- [x] 5.1 Ejecutar el launcher en modo regenerar (`-RegenerateScreenshots`) y confirmar que `BackgroundColorTest-expected.png` y `ModelTest-expected.png` se reescriben en `ProjectKristal.Windows/Screenshots/` con firma `\x89PNG` (no `JFIF`). Verificar: `git status` muestra esos dos ficheros modificados y la inspección de sus primeros bytes confirma PNG.
- [x] 5.2 Ejecutar el launcher en modo normal (sin el flag) y confirmar que `BackgroundColorTest` y `ModelTest` pasan su comparación con las nuevas bases PNG, y que `ComputeTest`/`EffectTest` no generan `.png`. Verificar: run sin excepciones de `EqualImages`; `bin/.../Screenshots` sin `.png` de tests no visuales.
- [x] 5.3 Actualizar `README.md` si menciona el formato de las capturas (`-expected.png`) o la librería de comparación (`ImageSharpCompare`), para reflejar PNG real con Skia y comparación propia. Verificar: el texto documentado concuerda con el comportamiento observado.

## Workflow follow-up

- Archivar el cambio (`openspec archive`) una vez satisfechos los requisitos de revisión del proyecto.
- Verificar el resultado archivado con `openspec show "visual-tests" --type spec`.
