# NetGuard GT - API REST de gestión de incidentes

Prototipo desarrollado en ASP.NET Core 9 para administrar incidentes de una red
de telecomunicaciones. Implementa las reglas del examen final de Análisis de
Sistemas I.

## Funcionalidad

- Registro y consulta de incidentes.
- SLA calculado según severidad.
- Máximo de 3 incidentes no cerrados por técnico.
- Flujo: `Registered -> Assigned -> InProgress -> Resolved -> Closed`.
- Asignación, reasignación y liberación de técnicos.
- Validación de especialidad.
- Escalamiento automático de incidentes críticos o urgentes sin atender por 2 horas.
- Historial completo de cambios.
- Reportes por estado, severidad, carga de técnicos, escalados y vencidos.
- Swagger disponible en `/swagger`.

## Supuestos de SLA

El enunciado no define valores exactos. Para hacer la regla verificable se usaron:

| Severidad | Tiempo máximo |
|---|---:|
| Critical | 4 horas |
| Urgent | 8 horas |
| Medium | 24 horas |
| Low | 48 horas |

Los datos se guardan en memoria para mantener el prototipo simple. Se reinician al
detener la aplicación.

## Ejecutar localmente

Requisitos: .NET SDK 9.

```powershell
dotnet restore .\ApiPrueba\ApiPrueba.slnx
dotnet run --project .\ApiPrueba\ApiPrueba\ApiPrueba.csproj
```

Abrir `http://localhost:5151/swagger`.

## Ejecutar pruebas

```powershell
dotnet test .\ApiPrueba.slnx
```

Para generar cobertura:

```powershell
dotnet test .\ApiPrueba.slnx --collect:"XPlat Code Coverage
```

## Ejecutar con Docker

```powershell
docker compose up --build
```

La API queda disponible en `http://localhost:5151/swagger`.

## Despliegue en Render

1. Subir el repositorio a GitHub con el nombre `ApiPrueba`.
2. Entrar a Render y seleccionar **New > Blueprint**.
3. Conectar el repositorio.
4. Render detectará `render.yaml` y construirá el `Dockerfile`.
5. Esperar el estado **Live** y abrir la URL pública seguida de `/swagger`.

El endpoint `/` funciona como comprobación de salud.

## Endpoints principales

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/sites` | Lista sitios |
| GET/POST | `/api/technicians` | Lista o crea técnicos |
| GET/POST | `/api/incidents` | Lista o registra incidentes |
| GET | `/api/incidents/{id}` | Obtiene un incidente |
| PUT | `/api/incidents/{id}/assignment` | Asigna o reasigna |
| DELETE | `/api/incidents/{id}/assignment` | Libera al técnico |
| PUT | `/api/incidents/{id}/status` | Avanza el estado |
| GET | `/api/incidents/{id}/history` | Consulta historial |
| POST | `/api/incidents/escalations/run` | Ejecuta escalamiento |
| GET | `/api/reports/summary` | Reporte general |
| GET | `/api/reports/overdue` | Incidentes fuera de SLA |

Los valores de enum se envían como texto en inglés: `Critical`, `Urgent`,
`Medium`, `Low`, `FiberOptic`, `Microwave`, `ElectricalSystems`.

## Documentación de evaluación

- [Historias de usuario](docs/HISTORIAS_USUARIO.md)
- [Diagrama de secuencia](docs/DIAGRAMA_SECUENCIA.md)
- [Informe de uso de IA](docs/INFORME_IA.md)
- Colección de ejemplos: `ApiPrueba/ApiPrueba/ApiPrueba.http`

## Link a RENDER

https://apiprueba-muza.onrender.com

