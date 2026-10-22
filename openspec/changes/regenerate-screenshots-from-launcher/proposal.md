# Proposal

## Why

Regenerar las capturas de referencia (`-expected.png`) de los tests visuales es hoy un proceso manual: se ejecuta la app, se localiza el `.png` actual en el directorio de salida y se copia a mano a la carpeta fuente trackeada en git. Queremos que el propio launcher de Windows lo haga en un solo paso, pero solo para los tests que comparan imágenes —no para los que validan datos—.

## What Changes

- Se reorganiza la jerarquía de tests en tres clases: `SceneTest` (base, lógica compartida: cámara 1280x720, `ArrangeActScene`, captura a frame==2, `Assert` y `Pop`), `NonVisualTest` (tests sin comparación de imagen) y `VisualTest` (tests que comparan imágenes: añade `EqualImages()` y la captura). **BREAKING**: `VisualTest` deja de ser la base abstracta de todo y pasa a ser la rama "snapshot"; los tests existentes se reasignan a la rama que corresponda.
- El marker "¿este test compara imágenes?" pasa de implícito (quien llama a `EqualImages()`) al sistema de tipos: el launcher filtra por `is VisualTest`.
- Se sanea el contrato: `ArrangeActScene` monta la escena; `Assert()` comprueba. Esto mueve la llamada a `EqualImages()` de `ModelTest.ArrangeActScene` a `Assert()` y resuelve el `FIXME` de "compara la captura de la ejecución anterior" en `BackgroundColorTest`.
- El csproj del launcher hornea la ruta de la carpeta fuente de capturas (`Screenshots`) al ensamblado vía `AssemblyMetadata`, para que el runtime la conozca sin flags de ruta.
- El launcher gana el flag `-RegenerateScreenshots` (bool, sin argumento). En ese modo, los `VisualTest` escriben `{ClassName}-expected.png` directamente en la carpeta fuente (entrando al commit), no comparan, y la app termina tras el último test sin cargar `MyScene`.
- En modo normal, el comportamiento no cambia: los `VisualTest` escriben `{ClassName}.png` relativo al CWD y comparan contra el `-expected.png` copiado por build.

## Capabilities

### New Capabilities
- `visual-tests`: el marco de tests visuales — jerarquía que distingue tests de snapshot de tests no visuales, captura y comparación de imágenes, y el modo de regeneración de capturas invocado desde el launcher.

### Modified Capabilities
<!-- Ninguna: no existen specs previas en el proyecto. -->

## Impact

- `Evergine.VisualTests/VisualTest.cs`: se reestructura en `SceneTest` (base) + `NonVisualTest` + `VisualTest`. Es un cambio de ruptura para la librería, que aspira a ser paquete público (ver README).
- `ProjectKristal/VisualTests/*.cs`: `BackgroundColorTest` y `ModelTest` pasan a heredar de `VisualTest`; `ComputeTest` y `EffectTest` de `NonVisualTest`. Se mueve `EqualImages()` a `Assert()`.
- `ProjectKristal/MyApplication.cs`: el conjunto a recorrer pasa a ser "descendientes de `SceneTest`" (ambas ramas); el TODO de auto-descubrimiento pivota sobre `SceneTest`.
- `ProjectKristal.Windows/Program.cs`: nuevo flag `-RegenerateScreenshots`; baja el modo a un `Settings` en el container.
- `ProjectKristal.Windows/ProjectKristal.Windows.csproj`: `<AssemblyMetadata Include="ScreenshotsSourceDir" ...>`.
- `ProjectKristal/Screenshots/` fuente trackeada en git: pasa a recibir los `-expected.png` regenerados directamente.
