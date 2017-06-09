using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public struct MultipartContentMember
    {
        public MultipartContentMember(HttpContent content, string name = null, string fileName = null)
            :this()
        {
            this.Content = content;
            this.Name = name;
            this.FileName = fileName;
        }

        public HttpContent Content { get; set; }

        public string Name { get; set; }

        public string FileName { get; set; }
    }
}
