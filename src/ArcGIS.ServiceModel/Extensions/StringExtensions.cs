using System;
using System.Text;
using ArcGIS.ServiceModel.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace ArcGIS.ServiceModel
{
    public static class StringExtensions
    {
        public static ArcGISServerEndpoint AsEndpoint(this string relativeUrl)
        {
            return new ArcGISServerEndpoint(relativeUrl);
        }

        public static ArcGISServerAdminEndpoint AsAdminEndpoint(this string relativeUrl)
        {
            return new ArcGISServerAdminEndpoint(relativeUrl);
        }

        public static ArcGISOnlineEndpoint AsArcGISOnlineEndpoint(this string relativeUrl)
        {
            return new ArcGISOnlineEndpoint(relativeUrl);
        }

        public static AbsoluteEndpoint AsAbsoluteEndpoint(this string url)
        {
            return new AbsoluteEndpoint(url);
        }

        public static string AsRootUrl(this string rootUrl)
        {
            if (string.IsNullOrWhiteSpace(rootUrl)) throw new ArgumentNullException("rootUrl");
            rootUrl = rootUrl.TrimEnd('/');
            if (rootUrl.IndexOf("/rest/services") > 0) rootUrl = rootUrl.Substring(0, rootUrl.IndexOf("/rest/services"));
            if (rootUrl.IndexOf("/admin") > 0) rootUrl = rootUrl.Substring(0, rootUrl.IndexOf("/admin"));
            return rootUrl.Replace("/rest/services", "") + "/";
        }

        public static Dictionary<string, string> ParseQueryString(this string queryString)
        {
            if (string.IsNullOrWhiteSpace(queryString)) return new Dictionary<string, string>();

            // remove anything other than query string from url
            if (queryString.Contains("?"))
                queryString = queryString.Substring(queryString.IndexOf('?') + 1);

            return Regex.Split(queryString, "&")
                .Select(vp => Regex.Split(vp, "="))
                .ToDictionary(singlePair => singlePair[0], singlePair => singlePair.Length == 2 ? singlePair[1] : string.Empty);
        }

        public static string UrlEncode(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

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

        /// <summary>
        /// Converts a hex-encoded string to the corresponding byte array.
        /// </summary>
        /// <param name="hex">Hex-encoded string</param>
        /// <returns>Byte representation of the hex-encoded input</returns>
        public static byte[] HexToBytes(this string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return null;

            int length = hex.Length;

            if (length % 2 != 0)
            {
                length += 1;
                hex = "0" + hex;
            }

            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}
