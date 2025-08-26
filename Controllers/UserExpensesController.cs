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

        /// <summary>
        /// Settle debt between users (core feature)
        /// </summary>
        /// <param name="settleDebtDto">Debt settlement data</param>
        /// <returns>Settlement result</returns>
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

        /// <summary>
        /// Get simplified group debts
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>Simplified group debts</returns>
        [HttpGet("group/{groupId}/debts")]
        [ProducesResponseType(typeof(IEnumerable<SimplifiedDebtDto>), 200)]
        public async Task<ActionResult<IEnumerable<SimplifiedDebtDto>>> GetSimplifiedGroupDebts(int groupId)
        {
            var debts = await _userExpenseService.GetSimplifiedGroupDebtsAsync(groupId);
            return Ok(debts);
        }

        /// <summary>
        /// Get user debt details
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User debt details</returns>
        [HttpGet("user/{userId}/debts")]
        [ProducesResponseType(typeof(IEnumerable<UserDebtDetailDto>), 200)]
        public async Task<ActionResult<IEnumerable<UserDebtDetailDto>>> GetUserDebtDetails(int userId)
        {
            var debtDetails = await _userExpenseService.GetUserDebtDetailsAsync(userId);
            return Ok(debtDetails);
        }
    }
}