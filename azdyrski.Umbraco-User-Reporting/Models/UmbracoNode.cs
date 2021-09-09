using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace azdyrski.Umbraco.UserReports.Models
{
    public class UmbracoNode
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public int Id { get; set; }
        public int Level { get; set; }

        public static UmbracoNode GetFromContent(IContent content)
        {
            return new UmbracoNode() { Icon = content.ContentType.Icon, Id = content.Id, Name = content.Name, Level = content.Level };
        }

        public static UmbracoNode GetFromPublishedContent(IPublishedContent content)
        {
            return new UmbracoNode() { Icon = "icon-document", Id = content.Id, Name = content.Name, Level = content.Level };
        }

        public static UmbracoNode GetFromMedia(IMedia media)
        {
            return new UmbracoNode() { Icon = null, Id = media.Id, Name = media.Name, Level = media.Level };
        }

    }
}
