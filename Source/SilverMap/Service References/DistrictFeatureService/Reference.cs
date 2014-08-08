﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This code was auto-generated by Microsoft.Silverlight.ServiceReference, version 4.0.60310.0
// 
namespace SilverMap.DistrictFeatureService {
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="DistrictFeature", Namespace="http://schemas.datacontract.org/2004/07/Ptvag.Dawn.SilverMap.Web")]
    public partial class DistrictFeature : object, System.ComponentModel.INotifyPropertyChanged {
        
        private byte[] GeometryWkbField;
        
        private string IdField;
        
        private string NameField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte[] GeometryWkb {
            get {
                return this.GeometryWkbField;
            }
            set {
                if ((object.ReferenceEquals(this.GeometryWkbField, value) != true)) {
                    this.GeometryWkbField = value;
                    this.RaisePropertyChanged("GeometryWkb");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Id {
            get {
                return this.IdField;
            }
            set {
                if ((object.ReferenceEquals(this.IdField, value) != true)) {
                    this.IdField = value;
                    this.RaisePropertyChanged("Id");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PolyPoint", Namespace="http://schemas.datacontract.org/2004/07/Ptvag.Dawn.SilverMap.Web")]
    public partial class PolyPoint : object, System.ComponentModel.INotifyPropertyChanged {
        
        private double XField;
        
        private double YField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double X {
            get {
                return this.XField;
            }
            set {
                if ((this.XField.Equals(value) != true)) {
                    this.XField = value;
                    this.RaisePropertyChanged("X");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double Y {
            get {
                return this.YField;
            }
            set {
                if ((this.YField.Equals(value) != true)) {
                    this.YField = value;
                    this.RaisePropertyChanged("Y");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="DistrictFeatureService.IDistrictFeatureService")]
    public interface IDistrictFeatureService {
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IDistrictFeatureService/GetDistrictFeaturePoint", ReplyAction="http://tempuri.org/IDistrictFeatureService/GetDistrictFeaturePointResponse")]
        System.IAsyncResult BeginGetDistrictFeaturePoint(string layerName, double x, double y, System.AsyncCallback callback, object asyncState);
        
        System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> EndGetDistrictFeaturePoint(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IDistrictFeatureService/GetDistrictFeaturePolygon", ReplyAction="http://tempuri.org/IDistrictFeatureService/GetDistrictFeaturePolygonResponse")]
        System.IAsyncResult BeginGetDistrictFeaturePolygon(string layerName, System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.PolyPoint> polygon, System.AsyncCallback callback, object asyncState);
        
        System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> EndGetDistrictFeaturePolygon(System.IAsyncResult result);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDistrictFeatureServiceChannel : SilverMap.DistrictFeatureService.IDistrictFeatureService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetDistrictFeaturePointCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetDistrictFeaturePointCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature>)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetDistrictFeaturePolygonCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetDistrictFeaturePolygonCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature>)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DistrictFeatureServiceClient : System.ServiceModel.ClientBase<SilverMap.DistrictFeatureService.IDistrictFeatureService>, SilverMap.DistrictFeatureService.IDistrictFeatureService {
        
        private BeginOperationDelegate onBeginGetDistrictFeaturePointDelegate;
        
        private EndOperationDelegate onEndGetDistrictFeaturePointDelegate;
        
        private System.Threading.SendOrPostCallback onGetDistrictFeaturePointCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetDistrictFeaturePolygonDelegate;
        
        private EndOperationDelegate onEndGetDistrictFeaturePolygonDelegate;
        
        private System.Threading.SendOrPostCallback onGetDistrictFeaturePolygonCompletedDelegate;
        
        private BeginOperationDelegate onBeginOpenDelegate;
        
        private EndOperationDelegate onEndOpenDelegate;
        
        private System.Threading.SendOrPostCallback onOpenCompletedDelegate;
        
        private BeginOperationDelegate onBeginCloseDelegate;
        
        private EndOperationDelegate onEndCloseDelegate;
        
        private System.Threading.SendOrPostCallback onCloseCompletedDelegate;
        
        public DistrictFeatureServiceClient() {
        }
        
        public DistrictFeatureServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DistrictFeatureServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DistrictFeatureServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DistrictFeatureServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Net.CookieContainer CookieContainer {
            get {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    return httpCookieContainerManager.CookieContainer;
                }
                else {
                    return null;
                }
            }
            set {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    httpCookieContainerManager.CookieContainer = value;
                }
                else {
                    throw new System.InvalidOperationException("Unable to set the CookieContainer. Please make sure the binding contains an HttpC" +
                            "ookieContainerBindingElement.");
                }
            }
        }
        
        public event System.EventHandler<GetDistrictFeaturePointCompletedEventArgs> GetDistrictFeaturePointCompleted;
        
        public event System.EventHandler<GetDistrictFeaturePolygonCompletedEventArgs> GetDistrictFeaturePolygonCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> OpenCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> CloseCompleted;
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult SilverMap.DistrictFeatureService.IDistrictFeatureService.BeginGetDistrictFeaturePoint(string layerName, double x, double y, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetDistrictFeaturePoint(layerName, x, y, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> SilverMap.DistrictFeatureService.IDistrictFeatureService.EndGetDistrictFeaturePoint(System.IAsyncResult result) {
            return base.Channel.EndGetDistrictFeaturePoint(result);
        }
        
        private System.IAsyncResult OnBeginGetDistrictFeaturePoint(object[] inValues, System.AsyncCallback callback, object asyncState) {
            string layerName = ((string)(inValues[0]));
            double x = ((double)(inValues[1]));
            double y = ((double)(inValues[2]));
            return ((SilverMap.DistrictFeatureService.IDistrictFeatureService)(this)).BeginGetDistrictFeaturePoint(layerName, x, y, callback, asyncState);
        }
        
        private object[] OnEndGetDistrictFeaturePoint(System.IAsyncResult result) {
            System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> retVal = ((SilverMap.DistrictFeatureService.IDistrictFeatureService)(this)).EndGetDistrictFeaturePoint(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetDistrictFeaturePointCompleted(object state) {
            if ((this.GetDistrictFeaturePointCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetDistrictFeaturePointCompleted(this, new GetDistrictFeaturePointCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetDistrictFeaturePointAsync(string layerName, double x, double y) {
            this.GetDistrictFeaturePointAsync(layerName, x, y, null);
        }
        
        public void GetDistrictFeaturePointAsync(string layerName, double x, double y, object userState) {
            if ((this.onBeginGetDistrictFeaturePointDelegate == null)) {
                this.onBeginGetDistrictFeaturePointDelegate = new BeginOperationDelegate(this.OnBeginGetDistrictFeaturePoint);
            }
            if ((this.onEndGetDistrictFeaturePointDelegate == null)) {
                this.onEndGetDistrictFeaturePointDelegate = new EndOperationDelegate(this.OnEndGetDistrictFeaturePoint);
            }
            if ((this.onGetDistrictFeaturePointCompletedDelegate == null)) {
                this.onGetDistrictFeaturePointCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetDistrictFeaturePointCompleted);
            }
            base.InvokeAsync(this.onBeginGetDistrictFeaturePointDelegate, new object[] {
                        layerName,
                        x,
                        y}, this.onEndGetDistrictFeaturePointDelegate, this.onGetDistrictFeaturePointCompletedDelegate, userState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult SilverMap.DistrictFeatureService.IDistrictFeatureService.BeginGetDistrictFeaturePolygon(string layerName, System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.PolyPoint> polygon, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetDistrictFeaturePolygon(layerName, polygon, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> SilverMap.DistrictFeatureService.IDistrictFeatureService.EndGetDistrictFeaturePolygon(System.IAsyncResult result) {
            return base.Channel.EndGetDistrictFeaturePolygon(result);
        }
        
        private System.IAsyncResult OnBeginGetDistrictFeaturePolygon(object[] inValues, System.AsyncCallback callback, object asyncState) {
            string layerName = ((string)(inValues[0]));
            System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.PolyPoint> polygon = ((System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.PolyPoint>)(inValues[1]));
            return ((SilverMap.DistrictFeatureService.IDistrictFeatureService)(this)).BeginGetDistrictFeaturePolygon(layerName, polygon, callback, asyncState);
        }
        
        private object[] OnEndGetDistrictFeaturePolygon(System.IAsyncResult result) {
            System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> retVal = ((SilverMap.DistrictFeatureService.IDistrictFeatureService)(this)).EndGetDistrictFeaturePolygon(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetDistrictFeaturePolygonCompleted(object state) {
            if ((this.GetDistrictFeaturePolygonCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetDistrictFeaturePolygonCompleted(this, new GetDistrictFeaturePolygonCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetDistrictFeaturePolygonAsync(string layerName, System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.PolyPoint> polygon) {
            this.GetDistrictFeaturePolygonAsync(layerName, polygon, null);
        }
        
        public void GetDistrictFeaturePolygonAsync(string layerName, System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.PolyPoint> polygon, object userState) {
            if ((this.onBeginGetDistrictFeaturePolygonDelegate == null)) {
                this.onBeginGetDistrictFeaturePolygonDelegate = new BeginOperationDelegate(this.OnBeginGetDistrictFeaturePolygon);
            }
            if ((this.onEndGetDistrictFeaturePolygonDelegate == null)) {
                this.onEndGetDistrictFeaturePolygonDelegate = new EndOperationDelegate(this.OnEndGetDistrictFeaturePolygon);
            }
            if ((this.onGetDistrictFeaturePolygonCompletedDelegate == null)) {
                this.onGetDistrictFeaturePolygonCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetDistrictFeaturePolygonCompleted);
            }
            base.InvokeAsync(this.onBeginGetDistrictFeaturePolygonDelegate, new object[] {
                        layerName,
                        polygon}, this.onEndGetDistrictFeaturePolygonDelegate, this.onGetDistrictFeaturePolygonCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginOpen(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(callback, asyncState);
        }
        
        private object[] OnEndOpen(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndOpen(result);
            return null;
        }
        
        private void OnOpenCompleted(object state) {
            if ((this.OpenCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.OpenCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void OpenAsync() {
            this.OpenAsync(null);
        }
        
        public void OpenAsync(object userState) {
            if ((this.onBeginOpenDelegate == null)) {
                this.onBeginOpenDelegate = new BeginOperationDelegate(this.OnBeginOpen);
            }
            if ((this.onEndOpenDelegate == null)) {
                this.onEndOpenDelegate = new EndOperationDelegate(this.OnEndOpen);
            }
            if ((this.onOpenCompletedDelegate == null)) {
                this.onOpenCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnOpenCompleted);
            }
            base.InvokeAsync(this.onBeginOpenDelegate, null, this.onEndOpenDelegate, this.onOpenCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginClose(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginClose(callback, asyncState);
        }
        
        private object[] OnEndClose(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndClose(result);
            return null;
        }
        
        private void OnCloseCompleted(object state) {
            if ((this.CloseCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.CloseCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void CloseAsync() {
            this.CloseAsync(null);
        }
        
        public void CloseAsync(object userState) {
            if ((this.onBeginCloseDelegate == null)) {
                this.onBeginCloseDelegate = new BeginOperationDelegate(this.OnBeginClose);
            }
            if ((this.onEndCloseDelegate == null)) {
                this.onEndCloseDelegate = new EndOperationDelegate(this.OnEndClose);
            }
            if ((this.onCloseCompletedDelegate == null)) {
                this.onCloseCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnCloseCompleted);
            }
            base.InvokeAsync(this.onBeginCloseDelegate, null, this.onEndCloseDelegate, this.onCloseCompletedDelegate, userState);
        }
        
        protected override SilverMap.DistrictFeatureService.IDistrictFeatureService CreateChannel() {
            return new DistrictFeatureServiceClientChannel(this);
        }
        
        private class DistrictFeatureServiceClientChannel : ChannelBase<SilverMap.DistrictFeatureService.IDistrictFeatureService>, SilverMap.DistrictFeatureService.IDistrictFeatureService {
            
            public DistrictFeatureServiceClientChannel(System.ServiceModel.ClientBase<SilverMap.DistrictFeatureService.IDistrictFeatureService> client) : 
                    base(client) {
            }
            
            public System.IAsyncResult BeginGetDistrictFeaturePoint(string layerName, double x, double y, System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[3];
                _args[0] = layerName;
                _args[1] = x;
                _args[2] = y;
                System.IAsyncResult _result = base.BeginInvoke("GetDistrictFeaturePoint", _args, callback, asyncState);
                return _result;
            }
            
            public System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> EndGetDistrictFeaturePoint(System.IAsyncResult result) {
                object[] _args = new object[0];
                System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> _result = ((System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature>)(base.EndInvoke("GetDistrictFeaturePoint", _args, result)));
                return _result;
            }
            
            public System.IAsyncResult BeginGetDistrictFeaturePolygon(string layerName, System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.PolyPoint> polygon, System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[2];
                _args[0] = layerName;
                _args[1] = polygon;
                System.IAsyncResult _result = base.BeginInvoke("GetDistrictFeaturePolygon", _args, callback, asyncState);
                return _result;
            }
            
            public System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> EndGetDistrictFeaturePolygon(System.IAsyncResult result) {
                object[] _args = new object[0];
                System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature> _result = ((System.Collections.ObjectModel.ObservableCollection<SilverMap.DistrictFeatureService.DistrictFeature>)(base.EndInvoke("GetDistrictFeaturePolygon", _args, result)));
                return _result;
            }
        }
    }
}