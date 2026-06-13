# Informe de utilización de inteligencia artificial

## Herramienta

Se utilizó Codex como asistente de análisis, programación, pruebas y documentación.
Las decisiones finales y la revisión del resultado corresponden al estudiante.

## Prompts enviados

1. "Lee todo el proyecto y completa todos los requerimientos del examen mostrado
   en la imagen."
2. Contexto proporcionado: proyecto ASP.NET Core existente, archivo
   `compose.yaml` y fotografía completa del enunciado.

## Trabajo apoyado por IA

- Extracción de reglas y entregables desde la fotografía.
- Diseño de modelos, endpoints y servicio de dominio.
- Implementación de validaciones y escalamiento automático.
- Creación de pruebas xUnit.
- Elaboración de historias, README, despliegue y diagrama Mermaid.

## Correcciones y decisiones realizadas

- Se detectó que `compose.yaml` apuntaba a un Dockerfile inexistente y se corrigió.
- Se mantuvo almacenamiento en memoria porque SQLite era opcional y el objetivo es
  un prototipo demostrable.
- Como el examen no establece tiempos SLA exactos, se definieron valores explícitos:
  4, 8, 24 y 48 horas.
- Se interpretó "incidente activo" literalmente como todo incidente no cerrado;
  por ello un incidente resuelto aún cuenta hasta pasar a `Closed`.
- La liberación conserva el estado para respetar la prohibición de retroceder.

## Reflexión

La IA aceleró la creación del esqueleto y ayudó a convertir reglas narrativas en
casos verificables. Su resultado no debe aceptarse sin revisión: hubo que fijar
supuestos que el enunciado no define, comprobar rutas del proyecto y ejecutar
compilación y pruebas. El mayor valor fue producir una primera versión coherente
que después puede validarse con evidencia automática.

## Verificación

El proyecto se considera revisado cuando:

- `dotnet build` termina sin errores.
- `dotnet test` ejecuta todas las pruebas correctamente.
- `docker compose up --build` expone Swagger en el puerto 5151.
- Los endpoints pueden recorrerse desde Swagger siguiendo el flujo del diagrama.
