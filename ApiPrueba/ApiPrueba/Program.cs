var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/saludo", () =>
{
    return Results.Ok(new
    {
        mensaje = "Hola desde ApiPrueba",
        fecha = DateTime.Now
    });
});

app.Run();