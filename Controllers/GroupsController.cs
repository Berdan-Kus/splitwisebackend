using Microsoft.AspNetCore.Mvc;
using SplitwiseAPI.DTOs.GroupDTOs;
using SplitwiseAPI.DTOs.UserExpenseDTOs;
using SplitwiseAPI.Services.Interfaces;

namespace SplitwiseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        /// <summary>
        /// Get all groups (for dropdowns, group selection)
        /// </summary>
        /// <returns>List of all groups</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GroupResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetAllGroups()
        {
            var groups = await _groupService.GetAllGroupsAsync();
            return Ok(groups);
        }

        /// <summary>
        /// Create a new group
        /// </summary>
        /// <param name="createGroupDto">Group creation data</param>
        /// <returns>Created group</returns>
        [HttpPost]
        [ProducesResponseType(typeof(GroupResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<GroupResponseDto>> CreateGroup([FromBody] CreateGroupDto createGroupDto)
        {
            var group = await _groupService.CreateGroupAsync(createGroupDto);
            return CreatedAtAction(nameof(GetGroup), new { id = group.GroupId }, group);
        }

        /// <summary>
        /// Get group by ID
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns>Group details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GroupResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<GroupResponseDto>> GetGroup(int id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
                return NotFound($"Group with ID {id} not found");

            return Ok(group);
        }

        /// <summary>
        /// Get group members list
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns>List of group members</returns>
        [HttpGet("{id}/members")]
        [ProducesResponseType(typeof(IEnumerable<GroupMemberDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<GroupMemberDto>>> GetGroupMembers(int id)
        {
            if (!await _groupService.GroupExistsAsync(id))
                return NotFound($"Group with ID {id} not found");

            var members = await _groupService.GetGroupMembersAsync(id);
            return Ok(members);
        }

        /// <summary>
        /// Add user to group
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <param name="userId">User ID to add</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/members/{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddUserToGroup(int id, int userId)
        {
            try
            {
                var result = await _groupService.AddUserToGroupAsync(id, userId);
                if (!result)
                    return BadRequest("User is already in the group");

                return Ok(new { message = "User added to group successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Remove user from group
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <param name="userId">User ID to remove</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}/members/{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveUserFromGroup(int id, int userId)
        {
            try
            {
                var result = await _groupService.RemoveUserFromGroupAsync(id, userId);
                if (!result)
                    return NotFound("User not found in group");

                return Ok(new { message = "User removed from group successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get group balance summary (core feature)
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns>Group balance summary</returns>
        [HttpGet("{id}/balance")]
        [ProducesResponseType(typeof(GroupBalanceSummaryDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<GroupBalanceSummaryDto>> GetGroupBalanceSummary(int id)
        {
            try
            {
                var balanceSummary = await _groupService.GetGroupBalanceSummaryAsync(id);
                return Ok(balanceSummary);
            }
            catch (ArgumentException)
            {
                return NotFound($"Group with ID {id} not found");
            }
        }

        /// <summary>
        /// Get groups by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User's groups</returns>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<GroupResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetGroupsByUserId(int userId)
        {
            var groups = await _groupService.GetGroupsByUserIdAsync(userId);
            return Ok(groups);
        }
    }
}