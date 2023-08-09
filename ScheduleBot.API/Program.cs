using System.Collections;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using ScheduleBot.API.ActionFilters;
using ScheduleBot.API.Services;
using ScheduleBot.Resources.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMvc()
    .AddMvcOptions(x =>
    {
        x.Filters.Add(new ExceptionFilterAttribute());
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.CustomSchemaIds(y => y.Name);
    x.IncludeXmlComments(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Assembly.GetExecutingAssembly().GetName().Name + ".xml"));
});

builder.Services.AddScoped<IEnumerable<Group>, GroupListService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();