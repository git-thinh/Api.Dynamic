using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace Api.Dynamic
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var cache = new StoreCache();

            //config.Services.Replace(typeof(IAssembliesResolver), new CustomAssemblyResolver());
            config.Services.Replace(typeof(IHttpControllerSelector), new ControllersResolver(config, cache));

            //var container = new UnityContainer();
            //container.RegisterType<IUserRepository, DbUserRepository>(new HierarchicalLifetimeManager());
            //config.DependencyResolver = new UnityResolver(container);

            // Web API configuration and services
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute( 
                name: "DefaultApi",
                //routeTemplate: "api/{controller}/{id}", defaults: new { id = RouteParameter.Optional }
                routeTemplate: "api/{controller}/{_functional}", defaults: new { _functional = RouteParameter.Optional }
            );

            //ControllerConfig.Map.Add(typeof(TestController), settings =>
            //{
            //    settings.Formatters.Clear();
            //    settings.Formatters.Add(new JsonMediaTypeFormatter());
            //});

            //config.Services.Replace(typeof(IHttpControllerActivator), new PerControllerConfigActivator());
        }
    }
}
