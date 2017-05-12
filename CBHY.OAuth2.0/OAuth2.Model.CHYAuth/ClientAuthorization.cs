/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OAuth2.Models.CHYAuth
{
    public class ClientAuthorization 
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Display(Name="AuthorizationId")]
        public int AuthorizationId{ get; set; }

        [Required]
        [Display(Name="CreatedOnUtc")]
        public DateTime CreatedOnUtc{ get; set; }

        [Required]
        [Display(Name="ClientId")]
        public int ClientId{ get; set; }

        [Display(Name="UserId")]
        public int? UserId{ get; set; }

        [Display(Name="Scope")]
        public string Scope{ get; set; }

        [Display(Name="ExpirationDateUtc")]
        public DateTime? ExpirationDateUtc{ get; set; }

    }
}