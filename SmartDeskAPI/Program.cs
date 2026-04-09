using DotNetEnv;
using SmartDeskAPI.Interfaces;
using SmartDeskAPI.Services;
using SmartDeskAPI.Strategies;

Env.Load(".env");

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["Gemini:ApiKey"] = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

var allowedOrigin = Environment.GetEnvironmentVariable("ALLOWED_ORIGIN") ?? "http://localhost:4200";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<KnowledgeService>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<SentimentResponseLayer>();
builder.Services.AddSingleton<SentimentService>();
builder.Services.AddHttpClient<AiChatService>();
builder.Services.AddSingleton<IAiAdapter, AiChatService>();
builder.Services.AddSingleton<IResponseStrategy, AiResponseStrategy>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngular");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
