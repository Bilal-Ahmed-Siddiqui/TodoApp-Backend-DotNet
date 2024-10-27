using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Models;
using TodoApp.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace TodoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly TodoappContext _context;

        public TodoController(TodoappContext context)
        {
            _context = context;
        }

        // 1. CREATE a new todo
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TodoDTO>> CreateTodo([FromBody] TodoDTO todoDTO)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var userId = int.Parse(userIdClaim.Value);

            // Map TodoDTO to Todo entity
            var todo = new Todo
            {
                UserId = userId,   // Use only userId as a foreign key
                Title = todoDTO.Title,
                Description = todoDTO.Description,
                Completed = todoDTO.Completed
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();
            todoDTO.Id = todo.Id;
            return Ok(todoDTO);   // Return the DTO in the response, if necessary
        }

        // 2. READ all todos
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<TodoDTO>>> GetTodos()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var userId = int.Parse(userIdClaim.Value);
            var todos = await _context.Todos
            .Where(t => t.UserId == userId)
            .Select(t => new TodoDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Completed = t.Completed,
                UserId = t.UserId
            })
            .ToListAsync();

            return Ok(todos);
        }

        // 3. READ a single todo by Id
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoDTO>> GetTodoById(int id)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var userId = int.Parse(userIdClaim.Value);
            var todo = await _context.Todos
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(t => new TodoDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Completed = t.Completed,
                UserId = t.UserId
            }).FirstOrDefaultAsync();
            if (todo == null) return NotFound("Todo not found");

            return Ok(todo);
        }

        // 4. UPDATE an existing todo
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoDTO todoDTO)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var userId = int.Parse(userIdClaim.Value);

            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null) return NotFound("Todo not found");

            // Update the todo entity with DTO data
            todo.Title = todoDTO.Title;
            todo.Description = todoDTO.Description;
            todo.Completed = todoDTO.Completed ?? false;

            await _context.SaveChangesAsync();

            return Ok(todoDTO);
        }

        // 5. DELETE a todo
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            //Retrieve the user ID from the JWT claims
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var userId = int.Parse(userIdClaim.Value);

            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null) return NotFound("Todo not found");

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Todo deleted" });
        }
    }
}
