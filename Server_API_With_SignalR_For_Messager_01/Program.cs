using Microsoft.EntityFrameworkCore;
using Server_API_With_SignalR_For_Messager_01.Hubs;
using Server_API_With_SignalR_For_Messager_01.Services;
using WebSocketSharpServer.DbContext.DbModel;
using WebSocketSharpServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSingleton<UsersManager>();
builder.Services.AddScoped<MemberShipServices>();
builder.Services.AddScoped<MessageServices>();
builder.Services.AddScoped<ConversationServices>();
builder.Services.AddScoped<ProfileServices>();

// Database injection
builder.Services.AddDbContext<ApplicationDbModel>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),ServiceLifetime.Scoped);

builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:5209")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<MainHub>("/MainHub");
app.Run();
