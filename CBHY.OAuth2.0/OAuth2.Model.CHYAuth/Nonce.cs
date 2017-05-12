/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OAuth2.Models.CHYAuth
{
    public class Nonce 
    {

        [Key]
        [Required]
         [StringLength(500)]
        [Display(Name="Context")]
        public string Context{ get; set; }

        [Required]
         [StringLength(4000)]
        [Display(Name="Code")]
        public string Code{ get; set; }

        [Required]
        [Display(Name="Timestamp")]
        public DateTime Timestamp{ get; set; }

    }
}