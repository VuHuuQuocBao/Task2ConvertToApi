using Data.EF;
using Microsoft.EntityFrameworkCore;
using Task2ConvertToApi.BackendApi.BackGroundTask;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IWorker, Worker>();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<BackGroundCalculateWorkTimePerDay>();

var connectionString = builder.Configuration.GetConnectionString("Task2Db");

builder.Services.AddDbContext<Task2DbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
}, ServiceLifetime.Singleton);

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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