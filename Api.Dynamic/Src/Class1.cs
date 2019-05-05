using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Api.Dynamic
{
    interface IMyInterface
    {
        string Hello(string request);
    }

    public class InterfaceReader
    {
    }

    //public class MyServiceActivator : IHttpControllerActivator
    //{
    //    private readonly InterfaceReader _reader;
    //    private readonly HttpConfiguration _configuration;

    //    public MyServiceActivator(InterfaceReader reader, HttpConfiguration configuration)
    //    {
    //        _reader = reader;
    //        _configuration = configuration;
    //    }

    //    public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
    //    {
    //        //// Change the line below to whatever suits your needs.
    //        //var controller = _reader.CreateController(new MyImplementation());
    //        //return controller;
    //    }
    //}

    public static class ControllerConfig
    {
        public static Dictionary<Type, Action<HttpControllerSettings>> Map = new Dictionary<Type, Action<HttpControllerSettings>>();
    }

    //public class PerControllerConfigActivator : IHttpControllerActivator
    //{
    //    private static readonly DefaultHttpControllerActivator Default = new DefaultHttpControllerActivator();
    //    private readonly ConcurrentDictionary<Type, HttpConfiguration> _cache = new ConcurrentDictionary<Type, HttpConfiguration>();

    //    public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
    //    {
    //        HttpConfiguration controllerConfig;
    //        if (_cache.TryGetValue(controllerType, out controllerConfig))
    //        {
    //            controllerDescriptor.Configuration = controllerConfig;
    //        }
    //        else if (ControllerConfig.Map.ContainsKey(controllerType))
    //        {
    //            controllerDescriptor.Configuration = controllerDescriptor.Configuration.Copy(ControllerConfig.Map[controllerType]);
    //            _cache.TryAdd(controllerType, controllerDescriptor.Configuration);
    //        }

    //        var result = Default.Create(request, controllerDescriptor, controllerType);
    //        return result;
    //    }
    //}


}