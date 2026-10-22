# Tasks

## 1. Reestructurar la jerarquía de tests

- [x] 1.1 Crear `SceneTest` (base) en `Evergine.VisualTests/` con la vida compartida actual de `VisualTest`: `CreateScene` (cámara 1280x720), `ArrangeActScene(Entity)` abstract, `Update()` frame==2 que llama en orden a `SaveCapture()` (virtual, base=no-op), `Assert()` (virtual, base=∅) y `Pop()`. Verificar: compila y conserva el ciclo `SaveCapture`→`Assert`→`Pop`.
- [x] 1.2 Crear `NonVisualTest : SceneTest` (marcador vacío, sin captura). Verificar: compila y `SaveCapture()` sigue siendo no-op.
- [x] 1.3 Reconvertir el `VisualTest` existente en `VisualTest : SceneTest` con la rama snapshot: `EqualImages()` y override de `SaveCapture()` que escribe `{ClassName}.png` en modo normal. Verificar: compila y expone `EqualImages()` solo en `VisualTest`.

## 2. Reasignar los tests existentes

- [x] 2.1 Hacer que `BackgroundColorTest` y `ModelTest` hereden de `VisualTest`, con `Assert()` llamando a `EqualImages()`. Mover la llamada de `ModelTest.ArrangeActScene` a `Assert()`. Verificar: compila; `ArrangeActScene` solo monta la escena; `Assert()` compara.
- [x] 2.2 Hacer que `ComputeTest` y `EffectTest` hereden de `NonVisualTest`. Verificar: compila; ninguno llama a `EqualImages()` ni genera `.png`.
- [x] 2.3 Comprobar que el `FIXME` de `BackgroundColorTest` ("compara la captura de la ejecución anterior") queda resuelto por el nuevo orden `SaveCapture`→`Assert`. Verificar: al ejecutar en modo normal, la comparación usa la captura del frame actual.

## 3. Bakear la ruta fuente de capturas

- [x] 3.1 Añadir `<AssemblyMetadata Include="ScreenshotsSourceDir" Value="$(MSBuildProjectDirectory)\Screenshots" />` a `ProjectKristal.Windows.csproj`. Verificar: tras build, `ildasm`/reflect lee el atributo `AssemblyMetadata` con clave `ScreenshotsSourceDir` y valor apuntando a la carpeta fuente.
- [x] 3.2 Exponer en tiempo de ejecución (en `Program.cs` o un helper) una lectura de `ScreenshotsSourceDir` desde `Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()`. Verificar: la lectura devuelve la ruta fuente en una build local.

## 4. Modo regeneración en el launcher

- [x] 4.1 Añadir el flag `-RegenerateScreenshots` (bool, sin argumento) al `CmdParser` de `Program.cs`. Verificar: al lanzar con `-RegenerateScreenshots`, el flag se parsea a true; sin él, false.
- [x] 4.2 Registrar el modo en un `Settings` en el container (patrón ya usado en `MyApplication`) para que `MyApplication` y los tests lo lean. Verificar: `Settings` refleja el modo sin usar estáticos.
- [x] 4.3 En `MyApplication.Initialize()`, bajo `#if DEBUG`, filtrar solo los `VisualTest` cuando el modo regenerar esté activo (filtro `is VisualTest`); tras el último test, salir sin cargar `MyScene`. Verificar: en modo regenerar la app termina tras los tests y no muestra la escena.
- [x] 4.4 En `VisualTest.SaveCapture()`, en modo regenerar escribir `{ClassName}-expected.png` en la `ScreenshotsSourceDir` leída del assembly (no comparar, no `EqualImages`). Verificar: el archivo cae en la carpeta fuente trackeada en git.

## 5. Verificación integral

- [x] 5.1 Ejecutar el launcher en modo regenerar (`-RegenerateScreenshots`) y confirmar que se generan `BackgroundColorTest-expected.png` y `ModelTest-expected.png` en `ProjectKristal.Windows/Screenshots/`, y *no* se generan `.png` de `ComputeTest`/`EffectTest`. Verificar: `git status` muestra solo los `-expected.png` nuevos en la carpeta fuente.
- [x] 5.2 Ejecutar el launcher en modo normal (sin el flag) y confirmar que `BackgroundColorTest` y `ModelTest` pasan su comparación contra las nuevas bases, y que `ComputeTest`/`EffectTest` no generan `.png` en bin. Verificar: run sin excepciones de `EqualImages`; carpeta `bin/.../Screenshots` sin `.png` de tests no visuales.
- [x] 5.3 Actualizar el `README.md` documentando el flag `-RegenerateScreenshots` y el flujo de regeneración. Verificar: los pasos del documento se siguen tal cual y regeneran las capturas.
