# ci-build Specification

## Purpose

CI de GitHub Actions que compila la solución de Project Kristal en cada commit y cada pull request hacia `main`, para detectar rupturas del build antes de que lleguen a la rama principal.

## Requirements

### Requirement: La CI dispara en push a main y en pull request hacia main

El sistema SHALL ejecutar el workflow de compilación automáticamente ante cada push a la rama `main` y ante cada pull request cuyo destino sea `main`, y SHALL NOT ejecutarlo ante push a otras ramas que no sean `main`.

#### Scenario: Push a main dispara la CI
- **WHEN** se hace un push a la rama `main`
- **THEN** el workflow de compilación se ejecuta automáticamente

#### Scenario: Pull request hacia main dispara la CI
- **WHEN** se abre o actualiza un pull request con destino `main`
- **THEN** el workflow de compilación se ejecuta automáticamente

#### Scenario: Push a una rama que no es main no dispara la CI
- **WHEN** se hace un push a una rama distinta de `main`
- **THEN** el workflow de compilación no se ejecuta

### Requirement: La CI restaura las dependencias NuGet antes de compilar

El sistema SHALL restaurar todos los paquetes NuGet referenciados por la solución como paso previo a la compilación, de modo que las referencias a `Evergine.*` y demás paquetes estén resueltas antes de invocar el compilador.

#### Scenario: Restauración previa a la compilación
- **WHEN** el workflow de compilación se ejecuta
- **THEN** los paquetes NuGet se restauran antes del paso de compilación
- **AND** la compilación no falla por referencias sin resolver

### Requirement: La CI compila toda la solución

El sistema SHALL compilar la solución `ProjectKristal.Windows.sln` al completo, incluyendo los cuatro proyectos que la componen (`ProjectKristal.Windows`, `ProjectKristal`, `ProjectKristal.Editor` y `Evergine.VisualTests`), en configuración `Release`.

#### Scenario: Compilación de la solución completa
- **WHEN** el workflow de compilación se ejecuta tras un push a `main`
- **THEN** la solución `ProjectKristal.Windows.sln` se compila en configuración `Release`
- **AND** los cuatro proyectos se compilan correctamente

#### Scenario: Build roto marca la CI como fallida
- **WHEN** un cambio introducido en un push o PR produce un error de compilación
- **THEN** el workflow de compilación finaliza con estado de fallo
- **AND** la rama `main` o el PR reflejan el fallo sin intervención manual

### Requirement: La CI no ejecuta tests

El sistema SHALL NOT ejecutar tests visuales ni de ningún tipo en la CI, dado que los tests visuales requieren render determinista headless que depende del soporte a Warp aún no publicado por Evergine.

#### Scenario: Sin paso de ejecución de tests
- **WHEN** el workflow de compilación se ejecuta
- **THEN** no se ejecuta ningún comando de test (`dotnet test` u otro)
- **AND** el workflow finaliza tras la compilación

### Requirement: La CI cachea los paquetes NuGet entre ejecuciones

El sistema SHALL cachear el directorio global de paquetes NuGet (`~/.nuget/packages`) entre ejecuciones del workflow, de modo que los paquetes Evergine descargados en una ejecución se reutilicen en las siguientes sin volver a descargarlos íntegros.

#### Scenario: Caché reutilizada entre ejecuciones
- **WHEN** el workflow de compilación se ejecuta por segunda vez sin cambios en las referencias de paquetes
- **THEN** los paquetes NuGet se sirven desde la caché restaurada
- **AND** no se descargan de nuevo íntegros desde el origen
