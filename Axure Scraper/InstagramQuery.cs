using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Axure_Scraper
{

    public class InstagramQuery
    {
        public long id { get; set; }
        public int first { get; set; }
        public string after { get; set; }

        public override string ToString()
        {
            var properties = from p in this.GetType().GetProperties()
                             where p.GetValue(this, null) != null
                             select "\"" + p.Name + "\":\"" + p.GetValue(this, null).ToString() + "\"";
            return HttpUtility.UrlEncode("{" + string.Join(",", properties.ToArray()) + "}");
        }
    }

}
