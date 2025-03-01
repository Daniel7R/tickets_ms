using TicketsMS.Application.Interfaces;
using TicketsMS.Application.Mapping;
using TicketsMS.Application.Services;
using TicketsMS.Domain.Enums;
using TicketsMS.Infrastructure.Data;
using TicketsMS.Infrastructure.EventBus;
using TicketsMS.Infrastructure.Repository;
using TicketsMS.Infrastructure.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SchemaFilter<EnumSchemaFilter>(); // Enables los enums as string
});
//to enable datetime
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddNpgsql<TicketDbContext>(builder.Configuration.GetConnectionString("dbConnectionTickets"));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IGenerateTicket, TicketService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddAutoMapper(typeof(MapperProfile));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IEventBusConsumer, EventBusConsumer>();
builder.Services.AddHostedService<EventBusConsumer>();

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
