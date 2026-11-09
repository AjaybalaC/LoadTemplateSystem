using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using LoadTemplateSystem.Web.Data;
using LoadTemplateSystem.Web.Services;
using LoadTemplateSystem.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Swagger / OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Load Template System API",
        Version = "v1",
        Description = "RESTful APIs for Load Templates, SKUs (Products), and SKU UOMs in WINIT Mobile Load Request Template System.",
        Contact = new OpenApiContact
        {
            Name = "WINIT Mobile Admin",
            Email = "admin@winit.com"
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure AppDbContext with MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySQL(connectionString);
});

// Register application services
builder.Services.AddScoped<ILoadTemplateService, LoadTemplateService>();
builder.Services.AddScoped<ISkuService, SkuService>();

var app = builder.Build();

// Seed initial SKU and LoadTemplate data (auto-creates database and tables if not exists)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(context);
}

// Enable Swagger in all environments for testing/demonstration
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Load Template System API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Load Template System - Swagger API Docs";
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/LoadTemplate/Index");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LoadTemplate}/{action=Index}/{id?}");

app.Run();
