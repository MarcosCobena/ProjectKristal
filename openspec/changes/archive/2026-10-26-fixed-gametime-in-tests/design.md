# Design

## Context

Ver `proposal.md - Why` para la motivación. Estado actual relevante, verificado descompilando el SDK de Evergine (`Evergine.Framework`/`Evergine.Components`, versión `2026.10.14.1-visualtests`):

- El `gameTime` se origina en [Program.cs](/ProjectKristal.Windows/Program.cs) con un `Stopwatch` real: `gameTime = clockTimer.Elapsed; clockTimer.Restart();` (líneas 64, 72-73). El render loop del SDK (`Evergine.Forms.RenderLoop.Run`) ejecuta el callback cuando la app está idle, sin sleep ni paso fijo; solo el VSync (activo por defecto) lo ancla a la tasa de refresco del monitor.
- El flujo del delta, confirmado descompilando, es: `Application.UpdateFrame(gameTime)` → `Clock.Update(gameTime)` + `ScreenContextManager.Update(gameTime)` → `Scene.BaseUpdate(gameTime)` copia `gameTime.Ticks * Scene.Speed` al campo privado `sceneTime` → `Scene.Update(sceneTime)` actualiza los managers (incluido `BehaviorManager`, que llama a `Spinner.Update(sceneTime)`).
- `Spinner.Update` (descompilado de `Evergine.Components.Graphics3D.Spinner`): `var val = (float)gameTime.TotalSeconds * AxisIncrease; transform.LocalOrientation *= Quaternion.CreateFromYawPitchRoll(val.Y, val.X, val.Z);`. La rotación por frame es proporcional al delta real.
- El test captura en el frame 2 ([SceneTest.cs](/Evergine.VisualTests/SceneTest.cs), `frameCount == 2`, línea 56). Al acumular dos deltas reales, el ángulo del modelo varía entre ejecuciones y `EqualImages()` falla.
- `MyApplication` ya hereda de `Application` y se registra/usa en `Program.Main`; es `MyApplication` la que llama `application.UpdateFrame(gameTime)` (línea 75). `Application.UpdateFrame` es `public virtual`.
- `ScreenContextManager.CurrentContext` es `public`; `ScreenContext` expone sus escenas por un indexador `public Scene this[int i]` y `public int Count`, y `Scenes` es interno. La base de tests `SceneTest` hereda de `Scene`.

## Goals / Non-Goals

**Goals:**
- Que la captura de cualquier test con movimiento dependiente del tiempo sea idéntica píxel a píxel entre ejecuciones, en la misma máquina.
- Que el mecanismo no toque la lógica del test ni el SDK; vive enteramente en el host del proyecto.

**Non-Goals:**
- Determinismo entre máquinas distintas (GPU, driver y texturizado propios de cada equipo quedan fuera).
- Paso fijo para la escena de producción `MyScene`.
- Cambiar el momento de captura (sigue siendo frame 2) ni la librería de comparación.
- Eliminar el `Stopwatch` de `Program.Main` (solo se ignora su valor bajo test).

## Decisions

### Decisión 1: el punto de sustitución es `Application.UpdateFrame`

**Elección:** `MyApplication` sobrescribe `Application.UpdateFrame(TimeSpan gameTime)` y, cuando la escena activa sea un `SceneTest`, sustituye `gameTime` por `TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60)` antes de `base.UpdateFrame(...)`:

```
MyApplication.UpdateFrame(gameTime):
    fixed = IsTestActive() ? TimeSpan.FromSeconds(1.0/60.0) : gameTime
    base.UpdateFrame(fixed)
```

**Por qué aguas arriba y no dentro del `Update` del test:** `Scene.BaseUpdate` copia el delta al campo `sceneTime` *antes* de dispatchearlo a los managers (`BehaviorManager` → `Spinner`). Sobrescribir el parámetro en `SceneTest.Update` llegaría tarde: el `Spinner` ya habría corrido con el delta real. El único seam donde el delta se controla *antes* de que la escena lo consuma es `UpdateFrame`, que alimenta tanto a `Clock` como a `ScreenContextManager` (rama que llega a la escena).

**Por qué `UpdateFrame` y no `Program.Main`:** `Program` mide con `Stopwatch` el tiempo *real de frame* para todos los usos (debug, producción). Anclar la decisión "estamos en un test" en `Program` duplicaría el conocimiento de tests en el launcher. `MyApplication` ya es la composición de escenas (`Initialize` construye el stack de tests), así que es el dueño natural de "¿hay un test activo?".

**Alternativa descartada — paso fijo en `Program.Main`:** dejaría el launcher sabiendo de tests (hoy no lo sabe) y obligaría a inyectar la decisión por flags. Más acoplamiento, sin beneficio.

**Alternativa descartada — sobrescribir `gameTime` en `SceneTest.Update`:** llegado tarde (ver arriba).

### Decisión 2: el predicado "escena activa es un test" se resuelve por tipo

**Elección:** `IsTestActive()` inspecciona `screenContextManager.CurrentContext` y devuelve `true` si alguna de sus escenas es asignable a `SceneTest`:

```
IsTestActive():
    ctx = screenContextManager.CurrentContext
    if ctx == null: return false
    for i in [0..ctx.Count): if ctx[i] is SceneTest: return true
    return false
```

**Por qué sobre un flag de modo:** el detector por tipo es robusto frente al orden de escenas y al.stack de `ScreenContext` (los tests se pushan en `MyApplication.Initialize`; `MyScene` también se pusha pero al final, y su contexto solo queda activo tras los `Pop` de los tests). No requiere propagar estado extra.

**Cobertura de la guarda:** durante los tests, el contexto activo contiene un `SceneTest`, luego `Pop()` reemplaza por el siguiente test o, al final, por `MyScene`. Cuando `MyScene` es la activa y no quedan transiciones pendientes de tests, `IsTestActive()` devuelve `false` → `gameTime` real → producción intacta. El modo regeneración también pasa por aquí (los tests se pushan igual), por lo que regenerar y validar ven el mismo fotograma determinista. El único frame de `MyScene` que no cumple esta regla es el primer `UpdateFrame` tras `Initialize` mientras se procesan transiciones de tests pendientes; ver "Pending-transition edge" más abajo.

### Decisión 3: constante centralizada, valor 1/60 s

**Elección:** `TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60)`, calculado por expresión (no mágico). Se repite exactamente cada frame.

**Por qué 1/60:** es la cadencia de captura objetivo implícita del proyecto (60 FPS es la API de referencia y `RefreshRate = 60` ya aparece en `Program.cs` para el swapchain). Usar el mismo número coherencia la simulación con la frecuencia de presentación que la mente del proyecto asume como "un frame".

### Decisión 4: detección por constructor + OnDesactivatingScene (sin reflexión)

**Contexto descubierto en implementación:** el predicado de la Decisión 2 ("CurrentContext contiene un SceneTest") resulta insuficiente en la práctica. Cuando MyApplication.Initialize ejecuta los Push de los tests, Evergine encola las transiciones y las resuelve dentro de Application.UpdateFrame. En el primer UpdateFrame tras Initialize, CurrentContext todavía es 
ull y el primer Update del SceneTest (p. ej. ModelTest frame 1) recibe gameTime real (variable). Aunque el frame 2 recibe paso fijo, el acumulado en sceneTime del spinner ya incluía el delta real del frame 1, y la captura al frame 2 deja de ser determinista entre ejecuciones (verificado: tres regeneraciones consecutivas en modo -RegenerateScreenshots generaban tres hashes distintos).

**Opción descartada — reflexión sobre pendingCommands:** la primera versión leía la cola privada pendingCommands de ScreenContextManager para anticipar la activación, pero acoplaba la implementación a un detalle interno del SDK de Evergine.

**Elección:** SceneTest mantiene un contador estático s_activeSceneTestCount. Se incrementa en el constructor del SceneTest (llamado dentro de MyApplication.Initialize, antes de cualquier ase.UpdateFrame, donde ya se sabe que habrá tests) y se decrementa en el manejador del evento público ScreenContextManager.OnDesactivatingScene cuando la escena desactivada sea un SceneTest. MyApplication.UpdateFrame consulta SceneTest.IsAnyActive() (= contador > 0). Así:

- En el primer UpdateFrame tras Initialize, el contador ya es positivo (los 
ew ModelTest() / 
ew BackgroundColorTest() ya corrieron en Initialize) → paso fijo aplicado desde el primer frame → captura determinista.
- Cuando se procesa el último Pop del último test (cuyo OnDesactivatingScene se dispara dentro de ese UpdateFrame), el contador baja a 0 → a partir del siguiente UpdateFrame la producción (MyScene) recibe gameTime real.
- Para evitar fugas de manejadores, SceneTest sobrescribe Dispose(bool disposing) y se desuscribe del evento en la fase de disposing.

**Por qué no OnActivatingScene solo:** el evento se dispara *dentro* de ase.UpdateFrame, por lo que al evaluar IsAnyActive() *antes* de la llamada base el contador aún reflejaría el estado anterior. Con el constructor anticipando la activación, el contador ya está correcto antes del primer UpdateFrame. El decremento sí queda en OnDesactivatingScene porque solo debe volver a   cuando el último test haya sido retirado del contexto activo.

**Por qué la constante FixedTimeStep vive en SceneTest:** pertenece al contrato determinista de los tests visuales, no al host de la aplicación. Moverla al proyecto Evergine.VisualTests concentra en SceneTest la definición de "un frame fijo de simulación bajo test" y deja MyApplication como un mero puente que solo lee ese valor.

**No se usa reflexión:** el acomodo usa únicamente el contrato público de Evergine (OnDesactivatingScene, Dispose(bool) y asignación por tipo). Nada accede a pendingCommands ni a campos privados.

## Risks / Trade-offs

- **El SceneTest heredero podría no ser detectado si un test envuelve escenas.** → El contador cubre herencia (ModelTest : VisualTest : SceneTest): toda subclase de SceneTest suma al construir y resta al desactivar. Cualquier test que *no* derive de SceneTest queda fuera por definición.
- **CurrentContext podría ser 
ull en el primer UpdateFrame.** → No afecta: el contador se incrementa en el constructor del SceneTest, por lo que ya es positivo antes de cualquier UpdateFrame, sin necesidad de mirar CurrentContext.
- **Clock y ScreenContextManager reciben el mismo ixed delta.** → Intencionado: Clock.TotalTime también avanza en pasos fijos, lo que refuerza la reproducibilidad. Si un test dependiera de wall-clock real, se rompería, pero ningún test actual lo hace.
- **El determinismo es *por*, no *entre máquinas*.** → Diferencias de driver/GPU/texturizado pueden seguir causando deltas entre equipos; este cambio elimina solo la fuente de no-determinismo dependiente del tiempo, que es la causa raíz observada aquí.
- **Producción 100% intacta:** en mediciones de producción (modo normal, MyScene activa tras los tests) los 1284 frames analizados recibieron gameTime == step; 0 frames cayeron en el paso fijo. No hay trade-off observado en el escenario "Transición entre test y producción" del spec.
## Migration Plan

1. En `SceneTest` (proyecto `Evergine.VisualTests`): añadir el contador estático `s_activeSceneTestCount` (incrementado en el constructor, decrementado en el handler de `OnDesactivatingScene`), la suscripción/desuscripción y la constante pública `FixedTimeStep`. Exponer `IsAnyActive()`.
2. En `MyApplication.UpdateFrame`: delegar `base.UpdateFrame(SceneTest.IsAnyActive() ? SceneTest.FixedTimeStep : gameTime)`.
3. Ejecutar `ModelTest` dos veces en modo normal y verificar que ambas capturas son idénticas entre sí y contra la referencia.
4. Lanzar la aplicación en modo normal (sin tests) y verificar que `MyScene` conserva su animación con tiempo real.

Rollback: eliminar el override de `UpdateFrame` y deshacer los cambios en `SceneTest` restaura el comportamiento anterior (delta real en todos los contextos); las `-expected.png` commiteadas siguen siendo válidas para `BackgroundColorTest` (que no depende de `dt`), y `ModelTest` vuelve al estado no determinista previo.

## Open Questions

Ninguna que cambie specs, enfoque o tareas. El valor exacto del paso fijo (1/60) se confirma en implementación; si se quisiera otra cadencia, solo cambia la constante, no el diseño.
