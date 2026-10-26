# Proposal

## Why

No existe CI: nada verifica que la solución compila tras un commit o un PR, así que un cambio que rompe el build puede aterrizar en `main` sin que nadie lo detecte hasta que se compila en local. Queremos una CI que compile toda la solución (`ProjectKristal.Windows.sln`) en cada push a `main` y en cada PR hacia `main`, para proteger la rama principal de forma automática. Los tests visuales no pueden ejecutarse todavía en CI porque Evergine aún no ha publicado el soporte a Warp, así que este cambio se limita a la compilación y deja la ejecución de tests fuera del alcance a propósito.

## What Changes

- Se añade un workflow de GitHub Actions (`.github/workflows/build.yml`) sobre `windows-latest` que restaura los paquetes NuGet y compila `ProjectKristal.Windows.sln` en configuración `Release`.
- El workflow dispara en `push` a `main` y en `pull_request` hacia `main`.
- El workflow cachea los paquetes NuGet (`~/.nuget/packages`) entre ejecuciones para no descargar los paquetes Evergine (`2026.5.26.2553`) íntegros en cada run.
- Se instala el SDK de .NET 10 (preview) explícitamente con `actions/setup-dotnet`, ya que no es el SDK por defecto de la imagen `windows-latest`.
- **No** se ejecutan tests visuales ni de ningún tipo: Evergine aún no ha publicado el soporte a Warp para render determinista headless, y los tests visuales comparan con tolerancia cero píxel a píxel.

## Capabilities

### New Capabilities
- `ci-build`: la CI que compila la solución en cada commit — disparo, restauración de NuGet, compilación de los proyectos de la `.sln` y caché de paquetes.

### Modified Capabilities
<!-- Ninguna: no se alteran requisitos de `visual-tests`. Los tests visuales siguen fuera de la CI por la limitación de Warp, ya recogida en la motivación. -->

## Impact

- `.github/workflows/build.yml` (nuevo): workflow de compilación.
- `openspec/specs/ci-build/spec.md` (nuevo): spec de la nueva capacidad.
- Sin afectación a código de aplicación: ningún `.cs`, `.csproj` ni `.sln` se modifica.
- Sin afectación a `visual-tests`: la CI no ejecuta tests; la capacidad existente queda intacta.
