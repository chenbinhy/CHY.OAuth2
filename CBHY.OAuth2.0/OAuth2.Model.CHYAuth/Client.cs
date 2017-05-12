/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OAuth2.Models.CHYAuth
{
    public class Client 
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Display(Name="ClientId")]
        public int ClientId{ get; set; }

        [Required]
         [StringLength(50)]
        [Display(Name="ClientIdentifier")]
        public string ClientIdentifier{ get; set; }

         [StringLength(50)]
        [Display(Name="ClientSecret")]
        public string ClientSecret{ get; set; }

         [StringLength(4000)]
        [Display(Name="Callback")]
        public string Callback{ get; set; }

         [StringLength(4000)]
        [Display(Name="Name")]
        public string Name{ get; set; }

        [Required]
        [Display(Name="ClientType")]
        public int ClientType{ get; set; }

    }
}