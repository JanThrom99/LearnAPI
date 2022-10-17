using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/TodoItems")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoItemsController(TodoContext context)
        {
            _context = context;
        }
        #region API Methods
        /// <summary>
        /// Method for getting all the stored items
        /// </summary>
        /// <returns>
        /// - A Collection of all items
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
        {
            return await _context.TodoItems
                .Select(x => ItemToDTO(x))
                .ToListAsync();
        }
        /// <summary>
        /// Method for getting a specific item by ID.
        /// </summary>
        /// <param name="id"> The ID the user gives into the method.</param>
        /// <returns>
        /// - A TodoItemDTO object and a 201 Response.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItemDTO>> GetTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return ItemToDTO(todoItem);
        }
        /// <summary>
        /// Method which is used to update an existing item.
        /// </summary>
        /// <param name="id">The ID of the item the user wants to update.</param>
        /// <param name="todoItemDTO">The "new Info" the user wants to store in the existing item.</param>
        /// <returns>
        /// - A "BadRequest - 400 Response" if the ID isnt matchable
        /// - A "NotFound - 404 Response" if the existing item with the correct ID is null or if the update is complete and the save didnt work properly
        /// - A "NoContent - 204 Response" if the update worked and the old item now has the new values
        /// </returns> 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodoItem(long id, TodoItemDTO todoItemDTO)
        {
            if (id != todoItemDTO.Id)
            {
                return BadRequest();
            }

            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            todoItem.Name = todoItemDTO.Name;
            todoItem.IsComplete = todoItemDTO.IsComplete;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!TodoItemExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }
        /// <summary>
        /// Method for creating a new Item.
        /// </summary>
        /// <param name="todoItemDTO">The Item the user wants to create with its specific values.</param>
        /// <returns>
        /// - A TodoItemDTO object and a 201 response.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<TodoItemDTO>> CreateTodoItem(TodoItemDTO todoItemDTO)
        {
            var todoItem = new TodoItem
            {
                IsComplete = todoItemDTO.IsComplete,
                Name = todoItemDTO.Name
            };

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTodoItem),
                new { id = todoItem.Id },
                ItemToDTO(todoItem));
        }
        /// <summary>
        /// Method to delete the desired Item. 
        /// </summary>
        /// <param name="id">The ID of the item the user wants to delete</param>
        /// <returns>
        /// - A "NoContent - 204 Response" if the item has successfully been deleted.
        /// - A "NotFound - 404 Response" if the to be deleted item is null
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region other Methods
        /// <summary>
        /// Method which returns a bool that tells the user whether there is an item with the ID he gave into the method.
        /// </summary>
        /// <param name="id">The ID of the todoItem the user wants to receive/check.</param>
        /// <returns>
        /// - A bool
        /// </returns>
        private bool TodoItemExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
        /// <summary>
        ///  //TODO comment 
        /// </summary>
        /// <param name="todoItem"></param>
        /// <returns></returns>
        private static TodoItemDTO ItemToDTO(TodoItem todoItem) =>
            new TodoItemDTO
            {
                Id = todoItem.Id,
                Name = todoItem.Name,
                IsComplete = todoItem.IsComplete
            };
        #endregion
    }
}