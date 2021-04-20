using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageBrowser.Models.Serialization
{
    public class RequestLinkInfo
    {
        public string Type { get; set; }  // enum view, edit или embed
        public string Scope { get; set; } // optional - properties: anonymous or organization
    }

    public class LinkResponseInfo
    {
        public string Id { get; set; }
        public string[] Roles { get; set; }
        public Link Link { get; set; }
    }

    public class Link
    {
        public string Type { get; set; }
        public string Scope { get; set; }
        public string WebUrl { get; set; }
        public OneDriveApplication Application { get; set; }
    }

    public class OneDriveApplication
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }
}
