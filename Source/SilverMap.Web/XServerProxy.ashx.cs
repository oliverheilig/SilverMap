//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Net;
using System.Web;
using System.Web.Services;
using Ptvag.Dawn.SilverMap.Web.Properties;

namespace Ptvag.Dawn.SilverMap.Web
{
    /// <summary>
    /// This class just forwards an xMap request to another xMapServer instance.
    /// This can be useful to access an xMapServer behind a firewall, allow cross-domain and cross-scheme calls,
    /// hide your token, add some load balancing, etc. The SL-client can still use all xMap-features.
    /// see also http://stackoverflow.com/questions/697177/relaying-a-request-in-asp-net-forwarding-a-request
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class XServerProxy : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string url;
            // build a new web request with the same parameters
            switch(context.Request.Params["type"])
            {
                case "xmap":
                    url = "https://xmap-" + Settings.Default.MapCluster + ".cloud.ptvgroup.com/xmap/ws/XMap";
                    break;
                case "xroute":
                    url = "https://xroute-" + Settings.Default.MapCluster + ".cloud.ptvgroup.com/xroute/ws/XRoute";
                    break;
                case "xlocate":
                    url = "https://xlocate-" + Settings.Default.MapCluster + ".cloud.ptvgroup.com/xlocate/ws/XLocate";
                    break;
                case "xtour":
                    url = "https://xtour-" + Settings.Default.MapCluster + ".cloud.ptvgroup.com/xtour/ws/XTour";
                    break;
                case "xmapmatch":
                    url = "https://xmapmatch-" + Settings.Default.MapCluster + ".cloud.ptvgroup.com/xmapmatch/ws/XMapMatch";
                    break;
                default:
                    {
                        context.Response.StatusCode = 500;
                        context.Response.End();
                        return;
                    }
            }

            var original = context.Request;
            var newRequest = (HttpWebRequest)WebRequest.Create(url);
            newRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("xtok:" + Settings.Default.Token));
            newRequest.ContentType = original.ContentType;
            newRequest.Method = original.HttpMethod;
            newRequest.UserAgent = original.UserAgent;

            // copy the data
            original.InputStream.CopyTo(newRequest.GetRequestStream());

            // make the request
            var newResponse = newRequest.GetResponse();

            // copy the response data
            context.Response.ContentType = newResponse.ContentType;
            newResponse.GetResponseStream().CopyTo(context.Response.OutputStream);
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }

#if DeployOnDemoServer
    // Demo Server only supports .NET 3.5
    // implement the .NET 4.0 Stream.CopyTo() method as an extension method for .NET 3.5
    public static class StreamExtension
    {
        public static void CopyTo(this Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    output.Close();
                    return;
                }

                output.Write(buffer, 0, read);
            }
        }
    }
#endif
}
