# Spec Delta

## Purpose

El marco de tests visuales de Project Kristal: distingue los tests que comparan imágenes de los que no, captura y compara contra una referencia, y permite regenerar esas referencias desde el propio launcher sin pasos manuales.

## ADDED Requirements

### Requirement: Distinguir tests de captura de tests no visuales

El marco SHALL clasificar cada test en uno de dos grupos según su intención: tests que validan apariencia mediante comparación de imagen, y tests que validan otra cosa (datos, cómputo) sin comparar imagen.

#### Scenario: Un test que compara imágenes
- **WHEN** un test valida apariencia comparando una captura con una referencia
- **THEN** el sistema lo marca como test de captura, de forma que el launcher lo identifique sin inspeccionar su lógica internar
- **AND** el test expone la operación de comparación de imágenes

#### Scenario: Un test que no compara imágenes
- **WHEN** un test valida datos o cómputo sin necesidad de comparar imágenes
- **THEN** el sistema lo marca como test no visual
- **AND** no dispone de la operación de comparación de imágenes

### Requirement: Ciclo de un test

Cada test SHALL ejecutar un ciclo de tres fases en orden: montar la escena, capturar el fotograma en un momento determinado y comprobar el resultado.

#### Scenario: Las fases no se mezclan
- **WHEN** se ejecuta un test
- **THEN** el montaje de la escena ocurre antes que la captura
- **AND** la captura ocurre antes que la comprobación
- **AND** la comprobación nunca depende de una captura de una ejecución anterior del mismo test

### Requirement: Captura y comparación en modo normal

En modo normal, un test de captura SHALL escribir una captura actual y compararla contra la referencia preexistente copiada por el proceso de build.

#### Scenario: Captura actual junto a la referencia
- **WHEN** un test de captura se ejecuta en modo normal
- **THEN** escribe su captura actual en el directorio de salida en runtime
- **AND** compara esa captura contra la referencia copiada desde la fuente

#### Scenario: Tests no visuales no generan captura
- **WHEN** un test no visual se ejecuta en modo normal
- **THEN** no escribe archivo de captura alguno
- **AND** solo ejecuta su comprobación de datos o cómputo

### Requirement: Regeneración de referencias desde el launcher

El launcher SHALL ofrecer un modo de regeneración que, invocado por una opción de línea de comandos, sobrescribe las capturas de referencia de los tests de captura directamente en la carpeta fuente, sin pasos manuales posteriores.

#### Scenario: Solo se regeneran los tests que comparan imágenes
- **WHEN** se invoca el launcher en modo regeneración
- **THEN** solo los tests de captura generan una nueva referencia
- **AND** los tests no visuales no generan archivo alguno

#### Scenario: La referencia cae en la carpeta fuente
- **WHEN** el modo regeneración escribe una nueva referencia
- **THEN** el archivo se escribe en la carpeta fuente que el proyecto declara como origen de capturas
- **AND** ese archivo queda disponible para entrar al control de versiones sin copia manual

#### Scenario: No se compara durante la regeneración
- **WHEN** se invoca el modo regeneración
- **THEN** los tests no comparan contra referencias preexistentes
- **AND** no se emiten resultados de aprobado o rechazado

#### Scenario: Terminación tras regenerar
- **WHEN** todos los tests de captura han escrito su referencia
- **THEN** la aplicación termina sin cargar la escena final de la aplicación
