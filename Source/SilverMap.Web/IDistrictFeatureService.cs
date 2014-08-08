//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Runtime.Serialization;
using System.ServiceModel;
using System.Collections.Generic;

namespace Ptvag.Dawn.SilverMap.Web
{
    [ServiceContract]
    public interface IDistrictFeatureService
    {
        [OperationContract]
        IEnumerable<DistrictFeature> GetDistrictFeaturePoint(string layerName, double x, double y);

        [OperationContract]
        IEnumerable<DistrictFeature> GetDistrictFeaturePolygon(string layerName, List<PolyPoint> polygon);
    }

    [DataContract]
    public class DistrictFeature
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public byte[] GeometryWkb { get; set; }
        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class PolyPoint
    {
        [DataMember]
        public double X { get; set; }
        [DataMember]
        public double Y { get; set; }
    }
}
