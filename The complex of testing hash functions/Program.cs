using Microsoft.EntityFrameworkCore;
using The_complex_of_testing_hash_functions.Models;

var builder = WebApplication.CreateBuilder(args);

// ��������� ��������� MVC
builder.Services.AddControllersWithViews();

// ����������� Entity Framework Core � SQL Server
builder.Services.AddDbContext<HashTestingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ����������� middleware � �������������
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
