using ToDoGrpc.Data;

namespace ToDoGrpc.Services;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly AppDbContext dbContext;

    public ToDoService(AppDbContext _dbContext)
    {
        dbContext = _dbContext;
    }
}