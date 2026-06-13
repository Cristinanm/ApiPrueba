# Historias de usuario

## HU-01 Registrar incidente

**Como** operador de red, **quiero** registrar un incidente con sitio, severidad y
especialidad requerida, **para** iniciar su seguimiento.

**Criterios de aceptación**

- Al enviar datos válidos se crea con estado `Registered`.
- Se asigna una fecha límite según la severidad.
- Se genera el primer registro del historial.

## HU-02 Consultar incidentes

**Como** coordinador, **quiero** listar y filtrar incidentes, **para** conocer la
situación operativa.

**Criterios de aceptación**

- Se puede filtrar por estado, severidad y escalamiento.
- Los resultados se ordenan del más reciente al más antiguo.

## HU-03 Asignar técnico compatible

**Como** coordinador, **quiero** asignar un técnico de la especialidad requerida,
**para** asegurar que pueda resolver la falla.

**Criterios de aceptación**

- La asignación válida cambia `Registered` a `Assigned`.
- Una especialidad distinta devuelve conflicto y no modifica el incidente.

## HU-04 Limitar carga del técnico

**Como** jefe técnico, **quiero** limitar a tres los incidentes activos por técnico,
**para** distribuir equitativamente el trabajo.

**Criterios de aceptación**

- Se permiten hasta 3 incidentes cuyo estado no sea `Closed`.
- La cuarta asignación activa es rechazada.

## HU-05 Avanzar estado

**Como** técnico, **quiero** actualizar el estado en orden, **para** reflejar el
avance real.

**Criterios de aceptación**

- Sólo se acepta el siguiente estado de la secuencia.
- No se permite retroceder ni saltar estados.
- Cada avance queda en el historial.

## HU-06 Reasignar incidente

**Como** coordinador, **quiero** reasignar un incidente, **para** atender cambios
de turno o disponibilidad.

**Criterios de aceptación**

- El nuevo técnico debe tener la especialidad correcta y capacidad disponible.
- La reasignación no retrocede el estado actual.

## HU-07 Liberar incidente

**Como** técnico asignado, **quiero** liberar un incidente, **para** que otro
técnico pueda tomarlo.

**Criterios de aceptación**

- Se elimina la asignación sin cambiar el estado.
- La liberación y su comentario quedan en el historial.

## HU-08 Escalar falta de atención

**Como** supervisor, **quiero** que los incidentes críticos o urgentes se escalen
tras dos horas sin atención, **para** evitar incumplimientos graves.

**Criterios de aceptación**

- Sólo aplica a `Critical` o `Urgent` en estado `Registered`.
- Se marca `IsEscalated` y la fecha de escalamiento.
- El proceso automático se ejecuta cada minuto.

## HU-09 Consultar historial

**Como** auditor, **quiero** consultar el historial del incidente, **para** saber
quién realizó cada cambio y cuándo.

**Criterios de aceptación**

- La respuesta contiene estado anterior, nuevo estado, responsable, fecha y comentario.
- Los registros se presentan cronológicamente.

## HU-10 Reportar operación

**Como** gerente, **quiero** un resumen de incidentes, **para** tomar decisiones
de capacidad y cumplimiento.

**Criterios de aceptación**

- El reporte incluye totales por estado y severidad.
- Incluye escalados, vencidos y carga activa de cada técnico.

## HU-11 Consultar incidentes vencidos

**Como** supervisor, **quiero** listar incidentes fuera de SLA, **para** priorizar
su resolución.

**Criterios de aceptación**

- Sólo aparecen incidentes no resueltos ni cerrados cuya fecha límite ya pasó.
- Se ordenan por la fecha límite más antigua.

## HU-12 Administrar técnicos

**Como** administrador, **quiero** registrar y listar técnicos con especialidad,
**para** mantener disponible el personal asignable.

**Criterios de aceptación**

- El nombre es obligatorio y tiene al menos 3 caracteres.
- El listado muestra especialidad, estado activo y cantidad de incidentes activos.
