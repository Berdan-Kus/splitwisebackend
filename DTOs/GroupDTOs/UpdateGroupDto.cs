using System.ComponentModel.DataAnnotations;

namespace SplitwiseAPI.DTOs.GroupDTOs
{
    public class UpdateGroupDto
    {
        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters")]
        public string? Name { get; set; }
    }

    public class ManageGroupMembersDto
    {
        [Required]
        public List<int> UserIds { get; set; }
    }
}