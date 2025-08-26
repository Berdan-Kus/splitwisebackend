using System.ComponentModel.DataAnnotations;

namespace SplitwiseAPI.DTOs.GroupDTOs
{
    public class CreateGroupDto
    {
        [Required(ErrorMessage = "Group name is required")]
        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters")]
        public string Name { get; set; }

        // Grup oluştururken hangi kullanıcıları ekleyeceğimiz (opsiyonel)
        public List<int>? MemberUserIds { get; set; }
    }
}