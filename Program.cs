using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDB>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "TodoAPI";
    config.Title = "TodoAPI v1";
    config.Version = "v1";
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "TodoAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

app.MapGet("/", () => "bonjour");

app.MapGet("/todoitems", async (TodoDB db) => await db.Todos.ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDB db) => await db.Todos.Where(t => t.IsCompleted).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDB db) => 
    await db.Todos.FindAsync(id)
        is Todo todo ? Results.Ok(todo) : Results.NotFound());

app.MapPost("/todoitems", async (Todo todo, TodoDB db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDB db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsCompleted = inputTodo.IsCompleted;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDB db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.MapPatch("/todoitems/{id}", async (int id, TodoPatchDto inputTodo, TodoDB db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();
    if (inputTodo.Name is not null) todo.Name = inputTodo.Name;
    if (inputTodo.IsCompleted is not null) todo.IsCompleted = inputTodo.IsCompleted.Value;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();