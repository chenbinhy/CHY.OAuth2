/*
 * 本文件由根据实体插件自动生成，请勿更改
 * =========================== */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OAuth2.Models.CHYAuth
{
    public class SymmetricCryptoKey 
    {

        [Key]
        [Required]
         [StringLength(500)]
        [Display(Name="Bucket")]
        public string Bucket{ get; set; }

        [Required]
         [StringLength(4000)]
        [Display(Name="Handle")]
        public string Handle{ get; set; }

        [Required]
        [Display(Name="ExpiresUtc")]
        public DateTime ExpiresUtc{ get; set; }

        [Required]
        [Display(Name="Secret")]
        public byte[] Secret{ get; set; }

    }
}