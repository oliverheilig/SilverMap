//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.IO;
using System.Web;
using System.Web.Services;

namespace Ptvag.Dawn.SilverMap.Web
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class XmapBackgroundHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            // parse input paramters
            int left, top, right, bottom, width, height;
            if (!int.TryParse(context.Request.Params["left"], out left))
                throw (new ArgumentException("Invalid parameter"));
            if (!int.TryParse(context.Request.Params["top"], out top))
                throw (new ArgumentException("Invalid parameter"));
            if (!int.TryParse(context.Request.Params["right"], out right))
                throw (new ArgumentException("Invalid parameter"));
            if (!int.TryParse(context.Request.Params["bottom"], out bottom))
                throw (new ArgumentException("Invalid parameter"));
            if (!int.TryParse(context.Request.Params["width"], out width))
                throw (new ArgumentException("Invalid parameter"));
            if (!int.TryParse(context.Request.Params["height"], out height))
                throw (new ArgumentException("Invalid parameter"));

            MapMode mapMode;
            switch(context.Request.Params["mode"])
            {
                case "street":
                    mapMode = MapMode.Street;
                    break;
                default:
                    mapMode = MapMode.Background;
                    break;
            }

            context.Response.ContentType = "image/png";

            string cacheKey = string.Format("XMapTile{0}/{1}/{2}/{3}/{4}/{5}/{6}",
                left, top, right, bottom, width, height, mapMode);
            var ms = context.Cache[cacheKey] as MemoryStream;
            if (ms != null)
            {
                ms.WriteTo(context.Response.OutputStream);
            }
            else
            {
                // request image from ajax servlet
                var layer = new XmapRenderer("https://xmap-" + Properties.Settings.Default.MapCluster + ".cloud.ptvgroup.com/xmap/ws/XMap", mapMode);

                ms = layer.GetImage(left, top, right, bottom, width, height, 16);

                if (ms != null)
                {
                    ms.WriteTo(context.Response.OutputStream);

                    context.Cache[cacheKey] = ms;
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}
