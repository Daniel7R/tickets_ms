using TicketsMS.Application.Interfaces;
using TicketsMS.Application.Mapping;
using TicketsMS.Application.Services;
using TicketsMS.Domain.Enums;
using TicketsMS.Infrastructure.Data;
using TicketsMS.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//to enable datetime
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddNpgsql<TicketDbContext>(builder.Configuration.GetConnectionString("dbConnectionTickets"));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddAutoMapper(typeof(MapperProfile));

var app = builder.Build();

Console.WriteLine(TicketStatus.CANCELED);
Console.WriteLine(TicketStatus.CANCELED.GetType());
Console.WriteLine(TicketStatus.CANCELED.ToString().Equals("CANCELED"));

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
