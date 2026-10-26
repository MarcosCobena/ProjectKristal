# Tasks

## 1. Workflow de compilación

- [x] 1.1 Crear `.github/workflows/build.yml` con `name: build`, `on: { push: { branches: [main] }, pull_request: { branches: [main] } }` y `runs-on: windows-latest`. Verificar: el fichero existe y `actionlint` (o la pestaña Actions de GitHub al pushear) no reporta errores de sintaxis YAML.
- [x] 1.2 Añadir paso `actions/checkout@v4`. Verificar: tras un run del workflow, el checkout ocurre sin error.
- [x] 1.3 Añadir paso `actions/setup-dotnet@v4` con una versión de SDK de .NET 10 concreta. Verificar: tras el run, `dotnet --version` imprime `10.x` en el log del paso.
- [x] 1.4 Añadir paso `actions/cache@v4` para `~/.nuget/packages` con `key` derivada del hash de los `**/*.csproj` y `Directory.Build.props`. Verificar: tras el segundo run consecutivo, el log del paso de caché muestra `Cache hit` sin re-descargar los paquetes Evergine `2026.5.26.2553` íntegros.
- [x] 1.5 Añadir paso `dotnet restore ProjectKristal.Windows.sln`. Verificar: el restore finaliza sin error y resuelve las referencias a `Evergine.*` y `SkiaSharp`.
- [x] 1.6 Añadir paso `dotnet build ProjectKristal.Windows.sln -c Release --no-restore`. Verificar: el build finaliza con exit 0 y el log muestra los cuatro proyectos compilados (`ProjectKristal.Windows`, `ProjectKristal`, `ProjectKristal.Editor`, `Evergine.VisualTests`).

## 2. Verificación de la CI

- [ ] 2.1 Hacer un push a `main` y confirmar que el workflow `build` se ejecuta automáticamente y termina en verde. Verificar: la pestaña Actions muestra el run del workflow disparado por el push a `main`.
- [ ] 2.2 Abrir un PR hacia `main` y confirmar que el workflow `build` se ejecuta en el PR y termina en verde. Verificar: el PR muestra el chequeo `build` en estado passing.
- [ ] 2.3 Confirmar que hacer un push a una rama que no es `main` no dispara el workflow. Verificar: no aparece run del workflow `build` para ese push en la pestaña Actions.
- [ ] 2.4 Introducir un cambio que rompa la compilación en un PR (p. ej. un error de sintaxis C#) y confirmar que el workflow `build` finaliza en fallo. Verificar: el PR refleja el chequeo `build` en estado failing hasta que se revierte el cambio.

## Workflow follow-up

- Archivar el cambio tras satisfacer los requisitos de revisión del proyecto (`/openspec-archive-change`).
- Verificar el resultado archivado: `openspec show "ci-build" --type spec` devuelve la capacidad en el spec principal.
- (Opcional, fuera del cambio) Activar branch protection en GitHub para requerir `build` como status check obligatorio en `main`.
