using System.ComponentModel.DataAnnotations;
using pos_service.Models.Enums;

namespace pos_service.Models
{
    public class Permission
    {
        public int Id                                { get; set; }

        [Required]
        public PermissionType PermissionType         { get; set; 

        [MaxLength(250)]
        public string? Description                   { get; set; }

        [Required]
        public PermissionCatagory PermissionCatagory { get; set; }

        public string PermissionTypeName
        {
            get
            {
                return PermissionType.ToString();
            }
        }

       
        public string PermissionCatagoryName
        {
            get
            {
                return PermissionCatagory.ToString();
            }
        }
    }
}
