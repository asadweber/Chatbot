using Application;
using Infrastructure;
using Serilog;

// ============================================================================
// Application entry point and composition root.
//
// Wires up the ASP.NET Core MVC pipeline. Data access, the Semantic Kernel /
// Ollama integration (chat + embeddings/RAG), and repository/service
// registrations live in Infrastructure.AddInfrastructure /
// Application.AddApplication.
// ============================================================================

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(
    Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
    optional: false, reloadOnChange: true);


// Serilog config lives entirely in appsettings.json ("Serilog" section).
builder.Services.AddSerilog(cfg => cfg.ReadFrom.Configuration(builder.Configuration));

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Non-development: route unhandled exceptions to a friendly error page
    // and enforce HTTPS via HSTS.
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

// Serves static assets (wwwroot) via the new ASP.NET Core static asset pipeline.
app.MapStaticAssets();

// Default MVC route: /{controller}/{action}/{id?}, defaulting to Home/Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
