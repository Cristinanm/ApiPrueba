using ApiPrueba.Data;
using ApiPrueba.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IncidentStore>();
builder.Services.AddSingleton<IncidentService>();
builder.Services.AddHostedService<IncidentEscalationWorker>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ApiExceptionMiddleware>();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    servicio = "NetGuard GT - API de incidentes",
    documentacion = "/swagger",
    estado = "operativo"
}));

app.Run();

public partial class Program;
