namespace VideoVecka2;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/*
Todo:
  id: int
  title: string
  description: string
  completed: bool
  creationDate: DateTime

Skapa todos:
POST /api/todo
title, description

Radera todos:
DELETE /api/todo/{id}

Uppdatera todos:
PUT /api/todo/{id}
completed

Hämta todos:
GET /api/todos

*/


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<TodoDbContext>(
            options =>
                options.UseNpgsql(
                    "Host=localhost;Database=videovecka2;Username=postgres;Password=password"
                )
        );
        builder.Services.AddControllers();
        builder.Services.AddScoped<TodoService, TodoService>();

        var app = builder.Build();

        app.MapControllers();
        app.UseHttpsRedirection();

        app.Run();
    }
}

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool Completed { get; set; }
    public DateTime CreationDate { get; set; }

    public Todo(string title, string description)
    {
        this.Title = title;
        this.Description = description;
        this.Completed = false;
        this.CreationDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
    }
}

public class TodoDbContext : DbContext
{
    public DbSet<Todo> Todos { get; set; }

    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options) { }
}

public class CreateTodoDto
{
    public string Title { get; set; }
    public string Description { get; set; }

    public CreateTodoDto(string title, string description)
    {
        this.Title = title;
        this.Description = description;
    }
}

[ApiController]
[Route("api")]
public class TodoController : ControllerBase
{
    private TodoService todoService;

    public TodoController(TodoService todoService)
    {
        this.todoService = todoService;
    }

    [HttpPost("todo")]
    public IActionResult CreateTodo([FromBody] CreateTodoDto dto)
    {
        try
        {
            Todo todo = todoService.CreateTodo(dto.Title, dto.Description);
            return Ok(todo);
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
    }

    [HttpDelete("todo/{id}")]
    public IActionResult RemoveTodo(int id)
    {
        Todo? todo = todoService.RemoveTodo(id);
        if (todo == null)
        {
            return NotFound();
        }

        return Ok(todo);
    }

    [HttpPut("todo/{id}")]
    public IActionResult UpdateTodo(int id, [FromQuery] bool completed)
    {
        Todo? todo = todoService.UpdateTodo(id, completed);
        if (todo == null)
        {
            return NotFound();
        }

        return Ok(todo);
    }

    [HttpGet("todos")]
    public List<Todo> GetAllTodos()
    {
        return todoService.GetAllTodos();
    }
}

public class TodoService
{
    private TodoDbContext context;

    public TodoService(TodoDbContext context)
    {
        this.context = context;
    }

    public Todo CreateTodo(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title must not be null or empty");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description must not be null or empty");
        }

        Todo todo = new Todo(title, description);
        context.Todos.Add(todo);
        context.SaveChanges();
        return todo;
    }

    public Todo? RemoveTodo(int id)
    {
        Todo? todo = context.Todos.Find(id);
        if (todo == null)
        {
            return null;
        }

        context.Todos.Remove(todo);
        context.SaveChanges();

        return todo;
    }

    public Todo? UpdateTodo(int id, bool completed)
    {
        Todo? todo = context.Todos.Find(id);
        if (todo == null)
        {
            return null;
        }

        todo.Completed = completed;
        context.SaveChanges();

        return todo;
    }

    public List<Todo> GetAllTodos()
    {
        return context.Todos.ToList();
    }
}
