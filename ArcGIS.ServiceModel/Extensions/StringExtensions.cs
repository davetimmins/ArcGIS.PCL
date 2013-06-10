using System;
using System.Text;
using ArcGIS.ServiceModel.Common;

namespace ArcGIS.ServiceModel.Extensions
{
    public static class StringExtensions
    {
        public static ArcGISServerEndpoint AsEndpoint(this String relativeUrl)
        {
            return new ArcGISServerEndpoint(relativeUrl);
        }

        public static String UrlEncode(this String text)
        {
            if (String.IsNullOrEmpty(text)) return text;
            
            var sb = new StringBuilder();

            foreach (var charCode in Encoding.UTF8.GetBytes(text))
            {
                if (
                    charCode >= 65 && charCode <= 90        // A-Z
                    || charCode >= 97 && charCode <= 122    // a-z
                    || charCode >= 48 && charCode <= 57     // 0-9
                    || charCode >= 44 && charCode <= 46     // ,-.
                    )
                    sb.Append((char)charCode);
                else
                    sb.Append('%' + charCode.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
