using MediatR;
using RAG.API.Middleware;
using RAG.Application.Commands.IndexDocument;
using RAG.Domain.Interfaces;
using RAG.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "RAG System API",
        Version = "v1",
        Description = "Retrieval Augmented Generation powered by Qdrant and Gemini"
    });
});

builder.Services.AddMediatR(typeof(IndexDocumentCommand).Assembly);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var vectorRepository = scope.ServiceProvider
        .GetRequiredService<IVectorRepository>();
    await vectorRepository.EnsureCollectionExistsAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

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

