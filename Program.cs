using armada_test.Data;
using armada_test.IServices;
using armada_test.Middlewares;
using armada_test.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ISalesService, SalesService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(connectionString, sqlOptions => { sqlOptions.EnableRetryOnFailure(); }));


var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseStaticFiles();
app.MapControllers();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Run();