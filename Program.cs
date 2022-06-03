using DemoMinimalApi.Data;
using Microsoft.EntityFrameworkCore;
using DemoMinimalApi.Models;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MinimalContextDb>(options => options.UseSqlite("Data Source=fornecedores.db"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapGet("/fornecedor", async (MinimalContextDb context) =>
{
    await context.Fornecedores.ToListAsync();
});

app.MapGet("/fornecedor/{:id}", async (Guid id, MinimalContextDb context) =>
    await context.Fornecedores.FindAsync(id) is Fornecedor fornecedor ? Results.Ok(fornecedor) : Results.NotFound()
).Produces<Fornecedor>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound);

app.MapPost("/fornecedor", async (MinimalContextDb context, Fornecedor fornecedor) =>
{
    if (!MiniValidation.MiniValidator.TryValidate(fornecedor, out var errors))
        return Results.ValidationProblem(errors);
    context.Fornecedores.Add(fornecedor);
    var result = await context.SaveChangesAsync();
    return result > 0 ? Results.Created($"/fornecedor/{fornecedor.Id}", fornecedor) : Results.BadRequest("Houve um problema ao salvar o registro");
}).ProducesValidationProblem().Produces<Fornecedor>(StatusCodes.Status201Created).Produces(StatusCodes.Status400BadRequest);

app.MapPut("/fornecedor/{:id}", async (Guid id, MinimalContextDb context, Fornecedor fornecedor) =>
{
    var fornecedorBanco = await context.Fornecedores.FindAsync(id);
    if (fornecedorBanco == null) return Results.NotFound();
    if (!MiniValidation.MiniValidator.TryValidate(fornecedor, out var errors)) return Results.ValidationProblem(errors);
    context.Fornecedores.Update(fornecedor);
    var result = await context.SaveChangesAsync();
    return result > 0 ? Results.NoContent() : Results.BadRequest("Houve um problema ao salvar o registro");
}).ProducesValidationProblem().Produces<Fornecedor>(StatusCodes.Status200OK).Produces(StatusCodes.Status204NoContent);

app.MapDelete("/fornecedor/{:id}", async (Guid id, MinimalContextDb context) =>
{
    var fornecedor = await context.Fornecedores.FindAsync(id);
    if (fornecedor == null) return Results.NotFound();
    context.Fornecedores.Remove(fornecedor);
    var result = await context.SaveChangesAsync();
    return result > 0 ? Results.NoContent() : Results.BadRequest("Houve um problema ao salvar o registro");
}).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);

app.Run();
