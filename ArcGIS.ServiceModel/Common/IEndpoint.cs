using System;

namespace ArcGIS.ServiceModel.Common
{
    /// <summary>
    /// Represents a REST endpoint
    /// </summary>
    public interface IEndpoint
    {
        /// <summary>
        /// Relative url of the resource
        /// </summary>
        String RelativeUrl { get; }

        /// <summary>
        /// Check the url is complete (ignore the scheme)
        /// </summary>
        /// <param name="rootUrl"></param>
        /// <returns></returns>
        String BuildAbsoluteUrl(String rootUrl);
    }

    /// <summary>
    /// Represents an ArcGIS Server REST endpoint
    /// </summary>
    public class ArcGISServerEndpoint : IEndpoint
    {
        /// <summary>
        /// Creates a new ArcGIS Server REST endpoint representation
        /// </summary>
        /// <param name="relativePath">Path of the endpoint relative to the root url of the ArcGIS Server</param>
        public ArcGISServerEndpoint(String relativePath)
        {
            if (String.IsNullOrWhiteSpace(relativePath)) throw new ArgumentNullException("relativePath");
            RelativeUrl = relativePath.Trim('/');
            if (RelativeUrl.IndexOf("rest/services") > 0) RelativeUrl = RelativeUrl.Substring(RelativeUrl.IndexOf("rest/services"));
            RelativeUrl = RelativeUrl.Replace("rest/services/", "");
            RelativeUrl = "rest/services/" + RelativeUrl;

            System.Diagnostics.Debug.WriteLine("Created ArcGISServerEndpoint for " + RelativeUrl);
        }
        
        public String RelativeUrl { get; private set; }

        public String BuildAbsoluteUrl(String rootUrl)
        {
            if (String.IsNullOrWhiteSpace(rootUrl)) throw new ArgumentNullException("rootUrl");
            return !RelativeUrl.Contains(rootUrl.Substring(6)) && !RelativeUrl.Contains(rootUrl.Substring(6))
                       ? rootUrl + RelativeUrl
                       : RelativeUrl;
        }
    }

    /// <summary>
    /// Represents an ArcGIS Server Administration REST endpoint
    /// </summary>
    public class ArcGISServerAdminEndpoint : IEndpoint
    {
        /// <summary>
        /// Creates a new ArcGIS Server REST Administration endpoint representation
        /// </summary>
        /// <param name="relativePath">Path of the endpoint relative to the root url of the ArcGIS Server</param>
        public ArcGISServerAdminEndpoint(String relativePath)
        {
            if (String.IsNullOrWhiteSpace(relativePath)) throw new ArgumentNullException("relativePath");
            RelativeUrl = relativePath.Trim('/');
            if (RelativeUrl.IndexOf("admin") > 0) RelativeUrl = RelativeUrl.Substring(RelativeUrl.IndexOf("admin"));
            RelativeUrl = RelativeUrl.Replace("admin/", "");
            RelativeUrl = "admin/" + RelativeUrl;

            System.Diagnostics.Debug.WriteLine("Created ArcGISServerAdminEndpoint for " + RelativeUrl);
        }

        public String RelativeUrl { get; private set; }

        public String BuildAbsoluteUrl(String rootUrl)
        {
            if (String.IsNullOrWhiteSpace(rootUrl)) throw new ArgumentNullException("rootUrl");
            return !RelativeUrl.Contains(rootUrl.Substring(6)) && !RelativeUrl.Contains(rootUrl.Substring(6))
                       ? rootUrl + RelativeUrl
                       : RelativeUrl;
        }
    }

    public class ArcGISOnlineEndpoint : IEndpoint
    {
        /// <summary>
        /// Creates a new ArcGIS Online REST endpoint representation
        /// </summary>
        /// <param name="relativePath">Path of the endpoint relative to the root url of ArcGIS Online</param>
        public ArcGISOnlineEndpoint(String relativePath)
        {
            if (String.IsNullOrWhiteSpace(relativePath)) throw new ArgumentNullException("relativePath");
            RelativeUrl = relativePath.Trim('/');
            if (RelativeUrl.IndexOf("sharing/rest") > 0) RelativeUrl = RelativeUrl.Substring(RelativeUrl.IndexOf("sharing/rest"));
            RelativeUrl = RelativeUrl.Replace("sharing/rest/", "");

            System.Diagnostics.Debug.WriteLine("Created ArcGISOnlineEndpoint for " + RelativeUrl);
        }

        public String RelativeUrl { get; private set; }

        public String BuildAbsoluteUrl(String rootUrl)
        {
            if (String.IsNullOrWhiteSpace(rootUrl)) throw new ArgumentNullException("rootUrl");
            return !RelativeUrl.Contains(rootUrl.Substring(6)) && !RelativeUrl.Contains(rootUrl.Substring(6))
                       ? rootUrl + RelativeUrl
                       : RelativeUrl;
        }
    }
}
