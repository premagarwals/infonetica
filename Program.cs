using infonetica.Model;
using infonetica.Services;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<WorkflowService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var workflows = app.MapGroup("/workflows");

// Create workflow
workflows.MapPost("/", (WorkflowService service, Workflow workflow) =>
{
    try
    {
        var created = service.Create(workflow);
        return Results.Ok(created);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Get all workflows
workflows.MapGet("/", (WorkflowService service) =>
{
    var allWorkflows = service.GetAll();
    return Results.Ok(allWorkflows);
});

// Get workflow by ID
workflows.MapGet("/{id}", (WorkflowService service, int id) =>
{
    var wf = service.Get(id);
    return wf is not null ? Results.Ok(wf) : Results.NotFound();
});

// Execute action
workflows.MapPost("/{id}/execute/{actionId}", (WorkflowService service, int id, int actionId) =>
{
    try
    {
        service.PerformAction(id, actionId);
        var updated = service.Get(id);
        return Results.Ok(updated);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Add state to workflow
workflows.MapPost("/{id}/states", (WorkflowService service, int id, State state) =>
{
    try
    {
        service.AddState(id, state);
        var updated = service.Get(id);
        return Results.Ok(updated);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Toggle state enabled/disabled
workflows.MapPut("/{id}/states/{stateId}/toggle", (WorkflowService service, int id, int stateId, bool enable) =>
{
    try
    {
        service.ToggleState(id, stateId, enable);
        var updated = service.Get(id);
        return Results.Ok(updated);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Add action to workflow
workflows.MapPost("/{id}/actions", (WorkflowService service, int id, ActionRequest request) =>
{
    try
    {
        service.AddAction(id, request.Action, request.FromStateId);
        var updated = service.Get(id);
        return Results.Ok(updated);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();

// Request model for adding actions
public class ActionRequest
{
    public infonetica.Model.Action Action { get; set; } = new();
    public int FromStateId { get; set; }
}