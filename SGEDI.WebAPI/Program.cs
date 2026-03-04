using SGEDI.Application; // Para encontrar tus Handlers
using SGEDI.Infrastructure; // Para encontrar tu Base de Datos

var builder = WebApplication.CreateBuilder(args);

// 1. Agrega el generador de OpenAPI (Estándar .NET 10)
builder.Services.AddOpenApi();

// 2. Agrega Swagger (Interfaz visual)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// 3. Activa la interfaz solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Nuevo en .NET 10
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();