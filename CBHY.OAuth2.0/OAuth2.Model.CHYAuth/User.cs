/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OAuth2.Models.CHYAuth
{
    public class User 
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Display(Name="UserId")]
        public int UserId{ get; set; }

        [Required]
         [StringLength(150)]
        [Display(Name="OpenIDClaimedIdentifier")]
        public string OpenIDClaimedIdentifier{ get; set; }

         [StringLength(150)]
        [Display(Name="OpenIDFriendlyIdentifier")]
        public string OpenIDFriendlyIdentifier{ get; set; }

    }
}