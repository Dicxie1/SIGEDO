using Asistencia.Data;
using Asistencia.Documents;
using Asistencia.Models;
using Asistencia.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(option =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    option.Filters.Add(new AuthorizeFilter());
});

// Aceptar fechas Legacy behavior
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Register DbContext with PostgreSQL provider
builder.Services.AddDbContext<Asistencia.Data.ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = "/Account/Login";
    option.AccessDeniedPath = "/Account/AccessDenied";
    option.Cookie.Name = "SGA_Auth";
    option.ExpireTimeSpan = TimeSpan.FromHours(3);
});

builder.Services.AddScoped<Asistencia.Services.ReportTermService>();
builder.Services.AddScoped<Asistencia.Services.AttendanceService>();
builder.Services.AddScoped<Asistencia.Services.SyllabusService>();
builder.Services.AddScoped<Asistencia.Services.ClassroomService>();
builder.Services.AddScoped<Asistencia.Services.CourseService>();
builder.Services.AddScoped<Asistencia.Services.AcademicTermService>();
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
app.UseAuthentication(); 
app.UseAuthorization();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Llamamos al método estático que acabamos de crear
        await Asistencia.Data.DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al sembrar la base de datos.");
    }
}

app.Run();
