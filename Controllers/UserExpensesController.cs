using Microsoft.AspNetCore.Mvc;
using SplitwiseAPI.DTOs.UserExpenseDTOs;
using SplitwiseAPI.Services.Interfaces;

namespace SplitwiseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserExpensesController : ControllerBase
    {
        private readonly IUserExpenseService _userExpenseService;

        public UserExpensesController(IUserExpenseService userExpenseService)
        {
            _userExpenseService = userExpenseService;
        }

        [HttpPost("settle-debt")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<bool>> SettleDebt([FromBody] SettleDebtDto settleDebtDto)
        {
            try
            {
                var result = await _userExpenseService.SettleDebtAsync(settleDebtDto);
                return Ok(result);
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

        [HttpGet("group/{groupId}/debts")]
        [ProducesResponseType(typeof(IEnumerable<SimplifiedDebtDto>), 200)]
        public async Task<ActionResult<IEnumerable<SimplifiedDebtDto>>> GetSimplifiedGroupDebts(int groupId)
        {
            var debts = await _userExpenseService.GetSimplifiedGroupDebtsAsync(groupId);
            return Ok(debts);
        }

        [HttpGet("user/{userId}/debts")]
        [ProducesResponseType(typeof(IEnumerable<UserDebtDetailDto>), 200)]
        public async Task<ActionResult<IEnumerable<UserDebtDetailDto>>> GetUserDebtDetails(int userId)
        {
            var debtDetails = await _userExpenseService.GetUserDebtDetailsAsync(userId);
            return Ok(debtDetails);
        }
    }
}