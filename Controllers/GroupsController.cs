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

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GroupResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetAllGroups()
        {
            var groups = await _groupService.GetAllGroupsAsync();
            return Ok(groups);
        }

        [HttpPost]
        [ProducesResponseType(typeof(GroupResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<GroupResponseDto>> CreateGroup([FromBody] CreateGroupDto createGroupDto)
        {
            var group = await _groupService.CreateGroupAsync(createGroupDto);
            return CreatedAtAction(nameof(GetGroup), new { id = group.GroupId }, group);
        }

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

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<GroupResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetGroupsByUserId(int userId)
        {
            var groups = await _groupService.GetGroupsByUserIdAsync(userId);
            return Ok(groups);
        }
    }
}