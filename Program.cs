using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Asistencia.Services.Interfaces;
using Asistencia.Documents;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Aceptar fechas Legacy behavior
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Register DbContext with PostgreSQL provider
builder.Services.AddDbContext<Asistencia.Data.ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
builder.Services.AddScoped<Asistencia.Services.ReportTermService>();
builder.Services.AddScoped<Asistencia.Services.SyllabusService>();
    // En tu Program.cs o Startup.cs
builder.Services.AddScoped<IGradebookExportService, ExcelGradeBookDocument>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseStatusCodePagesWithRedirects("/Home/HandleError/{0}");
    app.UseHsts();
}
QuestPDF.Settings.License = LicenseType.Community;


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
