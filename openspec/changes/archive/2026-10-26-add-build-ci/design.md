# Design

## Context

La solución `ProjectKristal.Windows.sln` agrupa cuatro proyectos todos sobre .NET 10 (`net10.0` y `net10.0-windows`). El lanzador `ProjectKristal.Windows` es el único `.exe` y el único con TFM Windows; los otros tres son bibliotecas. Las dependencias Evergine tienen versión fija `2026.5.26.2553` y se sirven desde `nuget.org` (ver [nuget.config](/c:/Repositorios/ProjectKristal/nuget.config)). El build con `dotnet build` de la `.sln` funciona sin Evergine Studio instalado, lo que hace viable una CI sin tooling del editor. Ver la motivación en [proposal.md - Why](./proposal.md).

## Goals / Non-Goals

**Goals:**
- Detectar rupturas de compilación en `main` y en PRs hacia `main` antes de mergear.
- Restringir el disparo a `main` + PRs para no quemar runners Windows en cada commit de feature.
- Reutilizar paquetes NuGet entre ejecuciones para acelerar el build.

**Non-Goals:**
- Ejecutar tests visuales o de cualquier tipo. Bloqueado por el soporte a Warp en Evergine; se recogerá en un cambio futuro cuando Warp esté publicado.
- Compilar en más de una configuración. Solo `Release`.
- Publicar artefactos, hacer release ni desplegar.
- Lint, formato (`dotnet format`) o análisis de cobertura. Fuera de alcance.
- Proteger `main` con required status checks a nivel de GitHub. Esa configuración es de repo, no de código; se deja al usuario aplicarla si lo desea.

## Decisions

### D1: Runner `windows-latest` en lugar de `ubuntu-latest`

`ProjectKristal.Windows` tiene `TargetFramework` `net10.0-windows` y `UseWindowsForms=true`. Un runner Linux no podría compilarlo. El TFM Windows fuerza `runs-on: windows-latest`. Alternativa descartada: compilar solo los tres proyectos `net10.0` en Linux dejaría fuera al lanzador, que es parte central de lo que se quiere proteger.

### D2: `actions/setup-dotnet` con .NET 10 en lugar de MSBuild/Visual Studio

.NET 10 no es el SDK por defecto en la imagen `windows-latest`, así que hay que instalarlo explícitamente. `actions/setup-dotnet` es declarativo, cacheable y reproduce el `dotnet` que se usa en local. Alternativa descartada: `setup-msbuild` ataría el build al Visual Studio de la imagen y al `vswhere`; más opaco y menos portable. Se pasa la versión concreta de SDK de .NET 10 al action.

### D3: `dotnet build` de la `.sln` en un solo paso, sin `dotnet restore` separado

`dotnet build` restaura implícitamente si falta restore, pero el spec exige restauración como paso previo explícito. Se mantiene un `dotnet restore` de la `.sln` seguido de `dotnet build -c Release --no-restore`, para que el paso de restore sea observable y el de build no repita trabajo. Alternativa descartada: un único `dotnet build` mezclaría restore y compilación en un solo paso, dificultando diagnosticar si un fallo es de restauración o de compilación.

### D4: Caché de NuGet con `actions/cache@v4` sobre `~/.nuget/packages`

La clave de caché se deriva del hash de `**/packages.lock.json` si existiera, o en su defecto del hash de los `.csproj` y `Directory.Build.props`. Como no hay `packages.lock.json` en el repo, se usa el hash de los `.csproj` + `*.props` como aproximación: cambia cuando cambian las referencias. Alternativa descartada: `cache: true` integrado en `setup-dotnet` es más simple pero menos controlado sobre la clave; se prefiere `actions/cache` explícito por claridad de la invalidación.

### D5: Disparo `push` a `main` + `pull_request` hacia `main`

Acordado en exploración: protege `main` sin ejecutar CI en cada commit de feature branches. Se usa `branches: [main]` en ambos eventos. Alternativa descartada: disparar en todo push a cualquier rama daría más feedback pero multiplicaría el coste de runners Windows.

### D6: Configuración `Release` únicamente

`Release` es la configuración más representativa de lo que iría a producción y aumenta la confianza del chequeo. `Debug` añadiría tiempo de runner sin aportar cobertura de rupturas distinta. Revisar en el futuro si se quiere validar `Debug` también.

## Risks / Trade-offs

- [.NET 10 es preview] → `setup-dotnet` puede cambiar la cadena de versiones disponible. Mitigación: fijar una versión de SDK concreta (p. ej. `10.0.x`) en el workflow para que un cambio del canal preview no rompa el build de golpe; revisar al publicarse .NET 10 GA.
- [La caché basada en hash de `.csproj` puede invalidarse en exceso] → cualquier edición de un `.csproj`, aunque no toque paquetes, invalida la caché. Aceptado: los `.csproj` cambian poco, y la invalidación puntual es preferible a servir paquetes obsoletos.
- [No hay `packages.lock.json`] → la restauración no es totalmente determinista entre máquinas. Aceptado por ahora; si se quiere reproducibilidad estricta, se añadiría `RestorePackagesPath` y lock files en otro cambio.
- [Sin ejecución de tests, la CI no detecta regresiones de comportamiento] → aceptado y documentado: bloqueado por Warp. El alivio es que al menos el build se mantiene verde.
- [Sin required status checks de GitHub que protejan `main`] → la CI corre pero nada impide mergear con el chequeo en fallo. Mitigación: documentar en el README o notas del repo que el usuario debe activar branch protection si lo desea; fuera del alcance del código.

## Migration Plan

1. Añadir `.github/workflows/build.yml` con la configuración descrita.
2. Tras el primer push a `main` o el primer PR, verificar en la pestaña Actions que el workflow corre y se pone verde.
3. (Opcional, fuera del cambio) Activar branch protection en GitHub para requerir `build` como status check en `main`.

## Open Questions

- ¿Se quiere también `Debug` además de `Release`? Por defecto no. Se puede añadir como matriz si se quiere validar ambas.
- ¿Se quiere activar branch protection de GitHub para exigir el chequeo? Es decisión de repo, no de código; se deja al usuario.
