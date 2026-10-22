# Design

## Context

Ver `proposal.md - Why` para la motivación. Estado actual relevante:

- `Evergine.VisualTests/VisualTest.cs` es la base abstracta única: crea una cámara 1280x720, al frame==2 ejecuta `SaveToFile` del framebuffer y llama a `Assert()`. El guardado es *incondicional* para todos los tests.
- `EqualImages()` vive en esa base y compara `Screenshots/{ClassName}.png` contra `Screenshots/{ClassName}-expected.png`, rutas relativas al CWD (el bin del launcher en runtime).
- De los cuatro tests, solo `BackgroundColorTest` y `ModelTest` llaman a `EqualImages()`; `ModelTest` lo hace *dentro* de `ArrangeActScene` (fuera de sitio), lo que provoca el `FIXME` de "compara la captura de la ejecución anterior".
- `[MyApplication.cs](ProjectKristal/MyApplication.cs)` instancia los tests a mano bajo `#if DEBUG`, con un TODO de auto-descubrimiento, y empuja `MyScene` al final de la pila de escenas.
- `ProjectKristal.Windows.csproj` ya declara la carpeta fuente de capturas (`Screenshots\**\*`) y la copia al output con `CopyToOutputDirectory`. Esa misma carpeta fuente es la que tras la build recibe las `xpected.png` commiteadas. La carpeta fuente está trackeada en git; la de bin, no.

## Goals / Non-Goals

**Goals:**
- Que el marker "¿este test compara imágenes?" sea el sistema de tipos, no un flag o introspección.
- Que el modo regenerar escriba las `xpected.png` directamente en la carpeta fuente, sin copia manual.
- Que el contrato `ArrangeActScene` / `Assert` quede saneado, resolviendo el `FIXME` de comparar captura anterior.

**Non-Goals:**
- Auto-descubrimiento de tests (el TODO de `MyApplication` sigue sin resolverse en este cambio; solo se ajusta su eje a `SceneTest`).
- Saneamiento de la ruta CWD-relativa del *modo normal* (ya funciona y no se acopla a la estructura del repo).
- Soporte para máquinas distintas a la del checkout fuente en modo regenerar (regenerar implica build+run en el propio checkout).
- Cambiar el formato de imagen o la librería de comparación (`Codeuctivity.ImageSharpCompare`).

## Decisions

### Decisión 1: jerarquía TRI con base `SceneTest`

**Elección:** tres clases — `SceneTest` (base, vida compartida), `NonVisualTest` (rama sin imagen) y `VisualTest` (rama snapshot, hereda `EqualImages()` y la captura).

**Por qué sobre el marker booleano (`UsesImageComparison`):** el sistema de tipos expresa el intento en compilación. `EqualImages()` solo existe en `VisualTest`; un `NonVisualTest` no puede llamarlo ni por error. El filtro del launcher pasa a ser `is VisualTest` (barato, sin reflexión). Además invita a fijar el contrato limpio `ArrangeActScene`/`Assert`.

**Por qué TRI sobre BI:** el usuario valoró disponer de dos puertas de entrada explícitas (`VisualTest` / `NonVisualTest`) frente a "la base ya es no-visual por defecto". Coste: `NonVisualTest` es, de momento, un marcador vacío; se justifica por descubribilidad (la librería aspira a paquete público, ver README).

**Alternativas descartadas:**
- Marker booleano `virtual bool UsesImageComparison => false`: más simple, pero no impide el mal uso y no normaliza el contrato.
- Inferir por cuerpo de `Assert()` (¿llama a `EqualImages()`?): sin riesgo de olvido, pero frágil y caro (reflexión/análisis estático).

### Decisión 2: el marker es el tipo; el guardado es un template method

**Elección:** `SceneTest.Update()` (frame==2) llama, en orden, a `SaveCapture()` (virtual, base=no-op), `Assert()` (virtual, base=∅) y `Pop()`. `VisualTest` sobreescribe `SaveCapture()`.

```
SceneTest.Update()  frame==2:
   SaveCapture();    // base=no-op; NonVisualTest: no-op; VisualTest: override
   Assert();         // base=∅; NonVisualTest: su assert de datos; VisualTest: EqualImages()
   Pop();
```

**Por qué:** centra el ciclo en la base y ramifica solo el hook de captura, que es lo único que difiere. El orden `SaveCapture` → `Assert` garantiza que la comparación use la captura *del frame actual*, no de una ejecución anterior (resolución del `FIXME`).

### Decisión 3: el destino de la referencia se bakea en el csproj

**Elección:** el csproj del launcher hornea la ruta de la carpeta fuente de capturas en el ensamblado vía `AssemblyMetadata`:

```xml
<ItemGroup>
  <AssemblyMetadata Include="ScreenshotsSourceDir"
                    Value="$(MSBuildProjectDirectory)\Screenshots" />
</ItemGroup>
```

El runtime la lee desde `Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()` *solo en modo regenerar* y escribe ahí `{ClassName}-expected.png`. En modo normal no se necesita y no se acopla al repo.

**Por qué sobre un flag `-ScreenshotsDir <ruta>`:** la carpeta fuente ya está declarada en el csproj; exponer ese mismo valor evita una fuente de verdad duplicada y un flag que el usuario tendría que recordar. La ruta bakeada es la *misma* que el csproj usa para copiar fuente→bin, no puede desincronizarse.

**Por qué sobre auto-detección (subir niveles desde el bin):** frágil ante cambios de `OutputPath` o publicación single-file.

**Tradeoff honesto:** `$(MSBuildProjectDirectory)\Screenshots` es una ruta *absoluta* propia de la máquina que compila. Es correcta porque regenerar es una operación de desarrollo (build==run en el propio checkout, presente para commitear). No aplica a un exe publicado en otra máquina, que jamás fue el caso de uso.

### Decisión 4: forma del flag y propagación

**Elección:** flag `-RegenerateScreenshots` (bool, sin argumento) en `Program.cs`. Se baja a un `Settings` registrado en el container, que `MyApplication` y los tests leen para ramificar. En modo regenerar, `MyApplication` empuja solo los `VisualTest` (filtro `is VisualTest`) y, tras el último test, sale sin cargar `MyScene`.

**Por qué `Settings` en el container:** ya es el patrón del proyecto (`MyApplication` registra `Settings`); evita parámetros estáticos y mantiene el acoplamiento por DI.

## Risks / Trade-offs

- **`VisualTest` cambia de significado (rotura de la librería).** → Es un cambio de ruptura consciente; la librería aspira a público pero el README declara estado temprano. Se documenta en el proposal.
- **Ruta bakeada absoluta.** → Aceptada por el caso de uso (build==run en el checkout). Si en el futuro se quiere portable, hornearía ruta relativa y resolver en runtime contra `AppContext.BaseDirectory`, asumiendo la profundidad del `OutputPath`.
- **`SaveToFile` podría no haber escrito a disco antes de `EqualImages()`.** → El contrato exige que la captura y la comparación ocurran en el mismo frame, captura primero. Si `Camera3D.SaveToFile` es asíncrono (GPU→disco), la comparación leería un fichero incompleto. Punto a verificar en implementación: confirmar sincronía del flush o esperar antes de comparar.
- **`NonVisualTest` como marcador vacío hoy.** → Coste asumido por descubribilidad; no arrastra lógica por sí mismo.

## Migration Plan

1. Reestructurar `Evergine.VisualTests/VisualTest.cs` en `SceneTest` + `NonVisualTest` + `VisualTest` (mantener namespace y resto de la API pública razonablemente).
2. Reasignar la herencia de los cuatro tests existentes a su rama; mover `EqualImages()` de `ModelTest.ArrangeActScene` a `Assert()`.
3. Añadir `AssemblyMetadata` al csproj del launcher y el flag `-RegenerateScreenshots` + `Settings` en `Program.cs`.
4. Ejecutar una vez en modo regenerar para producir las `-expected.png` (incluida `ModelTest-expected.png`, que hoy falta) y commitearlas.
5. Validar modo normal: las comparaciones pasan con las nuevas bases.

Rollback: revertir el cambio restaura la base única `VisualTest`; las `-expected.png` commiteadas son compatibles con el flujo anterior (siguen siendo leídas por `EqualImages`).

## Open Questions

- Ninguna que cambie specs, enfoque o tareas. La sincronía de `SaveToFile` (ver Risks) se resuelve en implementación, no requiere decisión de diseño.
