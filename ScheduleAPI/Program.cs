using ScheduleToJSON;
using System.Collections;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped(typeof(IEnumerable<Group>), typeof(Schedule));

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

class Schedule : IEnumerable<Group>
{
    List<Group> groups;
    public Schedule()
    {
        groups = JsonSerializer.Deserialize<List<Group>>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "schedule.json"));
    }
    public IEnumerator<Group> GetEnumerator() => groups.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => groups.GetEnumerator();
}