using Serilog;
using System.Data;
using Npgsql;
using CoinManager.Data; // Add this if Procedures is in the CoinManager.Data namespace

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure Dapper by setting up IDbConnection as a scoped service
builder.Services.AddScoped<IDbConnection>((serviceProvider) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("ExchangeEvaluatorDB");
    return new NpgsqlConnection(connectionString);
});

// Register Procedures as a scoped service
builder.Services.AddScoped<Procedures>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The application should only use HTTPS redirection if it's accessible via HTTPS
    app.UseHsts();
}

// Set the path base for the application
app.UsePathBase("/coinmanager");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();