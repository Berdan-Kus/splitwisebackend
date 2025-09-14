using Microsoft.AspNetCore.Mvc;
using SplitwiseAPI.DTOs.ExpenseDTOs;
using SplitwiseAPI.Services.Interfaces;

namespace SplitwiseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ExpenseListDto>), 200)]
        public async Task<ActionResult<IEnumerable<ExpenseListDto>>> GetAllExpenses()
        {
            var expenses = await _expenseService.GetAllExpensesAsync();
            return Ok(expenses);
        }

        [HttpPost("simple")]
        [ProducesResponseType(typeof(ExpenseResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ExpenseResponseDto>> CreateSimpleExpense([FromBody] SimpleExpenseDto simpleExpenseDto)
        {
            try
            {
                var expense = await _expenseService.CreateSimpleExpenseAsync(simpleExpenseDto);
                return CreatedAtAction(nameof(GetExpense), new { id = expense.ExpenseId }, expense);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ExpenseResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ExpenseResponseDto>> GetExpense(int id)
        {
            var expense = await _expenseService.GetExpenseByIdAsync(id);
            if (expense == null)
                return NotFound($"Expense with ID {id} not found");

            return Ok(expense);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var result = await _expenseService.DeleteExpenseAsync(id);
            if (!result)
                return NotFound($"Expense with ID {id} not found");

            return NoContent();
        }

        [HttpGet("group/{groupId}")]
        [ProducesResponseType(typeof(IEnumerable<ExpenseListDto>), 200)]
        public async Task<ActionResult<IEnumerable<ExpenseListDto>>> GetExpensesByGroup(int groupId)
        {
            var expenses = await _expenseService.GetExpensesByGroupIdAsync(groupId);
            return Ok(expenses);
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<ExpenseListDto>), 200)]
        public async Task<ActionResult<IEnumerable<ExpenseListDto>>> GetExpensesByUser(int userId)
        {
            var expenses = await _expenseService.GetExpensesByUserIdAsync(userId);
            return Ok(expenses);
        }
    }
}