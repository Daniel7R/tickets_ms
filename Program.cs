using TicketsMS.Application.Interfaces;
using TicketsMS.Application.Mapping;
using TicketsMS.Application.Services;
using TicketsMS.Infrastructure.Data;
using TicketsMS.Infrastructure.EventBus;
using TicketsMS.Infrastructure.Repository;
using TicketsMS.Infrastructure.Swagger;
using DotNetEnv;
using TicketsMS.Application.Handlers;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

Env.Load();
var builder = WebApplication.CreateBuilder(args); 
builder.Configuration.AddEnvironmentVariables();
// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TicketsMS API", Version = "v1" });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.SchemaFilter<EnumSchemaFilter>(); // Enables los enums as string
});
//to enable datetime
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddNpgsql<TicketDbContext>(builder.Configuration.GetConnectionString("dbConnectionTickets"));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICustomTicketQueriesRepo, CustomTicketQueriesRepo>();
builder.Services.AddScoped<IGenerateTicket, TicketService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddAutoMapper(typeof(MapperProfile));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddScoped<TicketsHandler>();
builder.Services.AddSingleton<IEventBusConsumer, EventBusConsumer>();
builder.Services.AddSingleton<IEventBusProducer, EventBusProducer>();
builder.Services.AddHostedService<EventBusConsumer>();
builder.Services.AddHostedService<EventBusProducer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



app.Run();
