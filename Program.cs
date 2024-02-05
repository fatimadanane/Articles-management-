using Alimentations.Data;
using Alimentations.Models;
using Alimentations.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Data.SqlClient;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<DatabaseService>();

builder.Services.AddTransient<DepotModel>();

builder.Services.AddTransient<AlimentationModel>();
builder.Services.AddTransient<ArticleModel>();
builder.Services.AddScoped<ArticleRepository>();
builder.Services.AddScoped<AlimentationRepository>();
builder.Services.AddScoped<DepotRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
