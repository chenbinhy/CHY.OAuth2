using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBHY.OAuth2.Models
{
    /// <summary>
    ///  授权项
    /// </summary>
    public class AuthItemModel
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 授权项
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// 是否授权
        /// </summary>
        public bool IsAuthed { get; set; }
    }
}