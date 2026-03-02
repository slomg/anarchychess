using AnarchyChess.Ai;
using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Ai.Service.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProtoBuf.Grpc.Server;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(
        6969,
        listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        }
    );
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Async(a => a.Console())
    .CreateLogger();
builder.Services.AddSerilog();

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddScoped<IAiEngineService, AiEngineService>();

builder.Services.AddScoped<IAiEngine, AiEngine>();
builder.Services.AddSingleton<IBitMoveGenerator, BitMoveGenerator>();
builder.Services.AddSingleton<IMoveOrdering, MoveOrdering>();
builder.Services.AddSingleton<IEvaluator, Evaluator>();
builder.Services.AddSingleton<IEndgameFactorCalculator, EndgameFactorCalculator>();

builder.Services.AddSingleton<IEvaluatorFunction, ActivityEvaluator>();
builder.Services.AddSingleton<IEvaluatorFunction, AggressionEvaluator>();
builder.Services.AddSingleton<IEvaluatorFunction, KingSafetyEvaluator>();
builder.Services.AddSingleton<IEvaluatorFunction, MaterialEvaluator>();
builder.Services.AddSingleton<IEvaluatorFunction, MobilityEvaluator>();
builder.Services.AddSingleton<IEvaluatorFunction, PawnSpaceEvaluator>();
builder.Services.AddSingleton<IEvaluatorFunction, PawnStructureEvaluator>();
builder.Services.AddSingleton<IEvaluatorFunction, KingEndgameActivityEvaluator>();

builder.Services.AddCodeFirstGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AiEngineService>();
app.MapGet(
    "/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909"
);

app.Run();
