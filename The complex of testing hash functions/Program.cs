using Microsoft.EntityFrameworkCore;
using The_complex_of_testing_hash_functions.Interfaces;
using The_complex_of_testing_hash_functions.Logging;
using The_complex_of_testing_hash_functions.Models;
using The_complex_of_testing_hash_functions.Services;

const string LogFilePath = "Logs/app.log";
const string ConnectionStringName = "DefaultConnection";
const string DefaultRoutePattern = "{controller=Home}/{action=Index}/{id?}";

var builder = WebApplication.CreateBuilder(args);

// ��������� ��������
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<HashTestingContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(ConnectionStringName)
        ?? throw new InvalidOperationException($"������ ����������� '{ConnectionStringName}' �� �������.")));

// ����������� �������� ����������
builder.Services.AddScoped<RainbowTableService>();
builder.Services.AddScoped<INistTestingService, NistTestingService>();
builder.Services.AddScoped<IDiehardTestingService, DiehardTestingService>();

// ��������� �����������
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(new FileLoggerProvider(LogFilePath));

var app = builder.Build();

// ��������� ��������� middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// ��������� �������������
app.MapControllerRoute(
    name: "default",
    pattern: DefaultRoutePattern);

app.Run();