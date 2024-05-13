using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly AppDbContext dbContext;

    public ToDoService(AppDbContext _dbContext)
    {
        dbContext = _dbContext;
    }

    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
    {
        //validate request
        if (request.Title == string.Empty || request.Description == string.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Please provide valid object"));
        }

        //Automapper can be used, instead of creating manually
        //Also DTO layer can be included, will enhance later
        ToDoItem toDoItem = new ToDoItem()
        {
            Title = request.Title,
            Description = request.Description
        };

        await dbContext.AddAsync(toDoItem);
        await dbContext.SaveChangesAsync();

        return await Task.FromResult(new CreateToDoResponse
        {
            Id = toDoItem.Id
        });
    }

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invlid Argument"));
        }

        var result = await dbContext.ToDoItems.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Requested content not found"));

        }
        return await Task.FromResult(new ReadToDoResponse
        {
            Id = result.Id,
            Title = result.Title,
            Description = result.Description,
            ToDoStatus = result.ToDoStatus
        });
    }

    public override async Task<GetAllResponse> ListToDo(GetAllRequest request, ServerCallContext context)
    {

        var result = await dbContext.ToDoItems.ToListAsync();
        // if (result == null)
        // {
        //     throw new RpcException(new Status(StatusCode.NotFound, "Requested content not found"));
        // }

        GetAllResponse getAllResponse = new GetAllResponse();
        foreach (var item in result)
        {
            getAllResponse.ToDo.Add(new ReadToDoResponse()
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                ToDoStatus = item.ToDoStatus
            });
        }

        return await Task.FromResult(getAllResponse);

    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
    {
        if (request == null || request.Id <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid values"));
        }

        var todoItem = await dbContext.ToDoItems.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (todoItem == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Requested todo item not found, Todo id {request.Id}"));

        }
        todoItem.Title = request.Title;
        todoItem.ToDoStatus = request.ToDoStatus;
        todoItem.Description = request.Description;

        await dbContext.SaveChangesAsync();

        return await Task.FromResult(new UpdateToDoResponse
        {
            Id = todoItem.Id
        });

    }

    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
    {
        if (request == null || request.Id <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid values"));
        }

        var todoItem = await dbContext.ToDoItems.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (todoItem == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Requested todo item not found, Todo id {request.Id}"));

        }


        dbContext.Remove(todoItem);
        await dbContext.SaveChangesAsync();

        return await Task.FromResult(new DeleteToDoResponse
        {
            Id = todoItem.Id
        });

    }
}