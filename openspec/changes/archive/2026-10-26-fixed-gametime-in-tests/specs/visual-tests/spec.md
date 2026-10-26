# Spec Delta

## Purpose

Marco de tests visuales de Project Kristal: distingue tests que comparan imagen de los que no, captura y compara contra una referencia, permite regenerar esas referencias desde el launcher y garantiza que la captura de un test es reproducible píxel a píxel entre ejecuciones.

## ADDED Requirements

### Requirement: Paso de simulación fijo bajo test

El sistema SHALL avanzar la simulación de un test con un paso de tiempo fijo de 1/60 s por fotograma mientras la escena activa sea un test, en lugar del tiempo real transcurrido, de modo que el estado de la escena en el fotograma de captura sea idéntico entre ejecuciones aun cuando contenga movimiento dependiente del tiempo.

#### Scenario: Misma captura entre ejecuciones con movimiento
- **WHEN** un test con movimiento dependiente del tiempo (p. ej. una malla que rota según el `gameTime`) se ejecuta dos veces en modo normal
- **THEN** el sistema avanza su simulación el mismo paso fijo en ambas ejecuciones
- **AND** la captura resultante es idéntica píxel a píxel en ambas

#### Scenario: Constante determinista
- **WHEN** la escena activa es un test
- **THEN** el delta de simulación aplicado a cada fotograma es exactamente 1/60 s
- **AND** no depende del tiempo real transcurrido, de la tasa de refresco del monitor ni de la carga del sistema

### Requirement: El paso fijo no afecta a la producción

El sistema SHALL conservar el tiempo de simulación real fuera de los tests, de modo que la escena de la aplicación no se vea alterada por la determinismo de los tests.

#### Scenario: Escena de producción con tiempo real
- **WHEN** la escena activa no es un test
- **THEN** el sistema aplica el tiempo real transcurrido como delta de simulación
- **AND** no lo sustituye por ningún paso fijo

#### Scenario: Transición entre test y producción
- **WHEN** el contexto activo cambia de un test a la escena de la aplicación
- **THEN** el sistema deja de aplicar el paso fijo a partir de ese cambio
- **AND** la escena de producción se actualiza con su tiempo real habitual
