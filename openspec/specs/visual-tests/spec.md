# visual-tests Specification

## Purpose

Marco de tests visuales de Project Kristal: distingue tests que comparan imagen de los que no, captura y compara contra una referencia, permite regenerar esas referencias desde el launcher y garantiza que la captura de un test es reproducible píxel a píxel entre ejecuciones.

## Requirements

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

### Requirement: Las capturas se escriben como PNG sin compresión con pérdida

El sistema SHALL escribir toda captura de un test visual como una imagen PNG sin compresión con pérdida, de modo que los bytes del fichero resultante lleven la firma de PNG (`\x89PNG`) y nunca la firma JPEG (`JFIF`), con independencia del nombre o la extensión del fichero de salida.

#### Scenario: La captura lleva la firma PNG
- **WHEN** un test de captura escribe su captura en modo normal o de regeneración
- **THEN** los primeros bytes del fichero resultante corresponden a la firma PNG (`\x89PNG\r\n\x1a\n`)
- **AND** el fichero no contiene la marca JPEG `JFIF` en su cabecera

#### Scenario: La compresión con pérdida no degrada la captura
- **WHEN** dos capturas del mismo test se comparan entre ejecuciones
- **THEN** ninguna de las dos ha sufrido transformaciones con pérdida antes de la comparación
- **AND** cualquier diferencia detectada proviene del contenido de la escena, no del codec de imagen

### Requirement: La captura se obtiene leyendo la textura a la CPU por API pública

El sistema SHALL obtener los píxeles de la captura leyendo la textura del objetivo de color del framebuffer desde la GPU hasta la CPU usando exclusivamente la API pública del motor de gráficos, sin emplear reflexión sobre tipos internos del motor.

#### Scenario: Sin reflexión sobre el motor
- **WHEN** el sistema lee los píxeles de la textura capturada
- **THEN** la lectura se realiza mediante métodos públicos del motor (creación de una textura staging, copia de la textura fuente a la staging, mapeo a memoria de CPU y desmapeo)
- **AND** en ningún punto se usa reflexión para invocar miembros no públicos del motor ni para mutar su estado interno

#### Scenario: La lectura respeta el asincronismo de la GPU
- **WHEN** el sistema solicita la copia de la textura fuente a la staging
- **THEN** espera a que la copia se complete antes de mapear y leer la staging
- **AND** los píxeles leídos corresponden al estado de la textura en el fotograma de captura, no a un estado parcial o posterior

### Requirement: La comparación opera sobre los bytes de píxel, no sobre ficheros descodificados

El sistema SHALL comparar la captura actual contra la referencia interpretando directamente los bytes de píxel (RGBA) en memoria, sin depender de descodificar imágenes desde ficheros de disco mediante una librería externa de comparación.

#### Scenario: Comparación píxel a píxel con tolerancia exacta
- **WHEN** el sistema compara la captura actual contra la referencia
- **THEN** compara canal a canal los bytes RGBA de ambas imágenes
- **AND** la captura se considera igual a la referencia solo si todos los canales de todos los píxeles son idénticos (tolerancia cero)

#### Scenario: Sin dependencia externa de comparación de imágenes
- **WHEN** el sistema compara dos imágenes
- **THEN** la comparación se realiza con código propio del marco de tests
- **AND** no se invoca ninguna librería externa de comparación de imágenes para decidir el resultado
