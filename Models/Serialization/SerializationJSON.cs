using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageBrowser.Models.Serialization
{
    public class RequestLinkInfo
    {
        public string type { get; set; }  // enum view, edit или embed
        public string scope { get; set; } // optional - properties: anonymous or organization
    }

    public class LinkResponseInfo
    {
        public string id { get; set; }
        public string[] roles { get; set; }
        public Link link { get; set; }
    }

    public class Link
    {
        public string type { get; set; }
        public string scope { get; set; }
        public string webUrl { get; set; }
        public OneDriveApplication application { get; set; }
    }

    public class OneDriveApplication
    {
        public string id { get; set; }
        public string displayName { get; set; }
    }
}
