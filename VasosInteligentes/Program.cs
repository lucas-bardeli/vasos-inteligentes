using Microsoft.AspNetCore.Identity;
using VasosInteligentes.Data;
using VasosInteligentes.Models;
using VasosInteligentes.Seeds;
using VasosInteligentes.Services;
using VasosInteligentes.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Abre a conexão no banco de dados, usando as configurações do appsettings.json
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoConnection"));

// Define que é a única vez que estamos abrindo a conexão
builder.Services.AddSingleton<ContextMongoDb>();

// Configuração do Identity
var mongoSettings = builder.Configuration.GetSection("MongoConnection").Get<MongoSettings>();
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
}).AddMongoDbStores<ApplicationUser, ApplicationRole, string>(mongoSettings?.ConnectionString, mongoSettings?.Database)
.AddDefaultTokenProviders();

// Importante se formos utilizar Scaffolding para criar as Views de Login e Registro,
// pois o Scaffolding depende do Identity para criar as RazorPages
builder.Services.AddRazorPages();

// Configurações de envio de e-mail
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

// Adiciona as Seeds
using (var Scope = app.Services.CreateScope())
{
    var services = Scope.ServiceProvider;
    try
    {
        await IdentitySeeds.SeedRolesAndUser(services, "Admin@123");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro de Seed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

// Acrescentar app.UseAuthentication(); antes do app.UseAuthorization();
app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();