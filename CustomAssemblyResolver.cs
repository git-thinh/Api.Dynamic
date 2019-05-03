using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Api.Dynamic
{
    public class BypassCacheSelector : DefaultHttpControllerSelector
    {
        private readonly HttpConfiguration _configuration;
        private readonly ICache _cache;

        public BypassCacheSelector(HttpConfiguration configuration, ICache cache) : base(configuration)
        {
            _configuration = configuration;
            _cache = cache;
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            var assembly = Assembly.LoadFile(@"E:\Api.Dynamic\DLL\Api.Dynamic.Controllers.Test.dll");
            var types = assembly.GetTypes(); //GetExportedTypes doesn't work with dynamic assemblies
            var matchedTypes = types.Where(i => typeof(IHttpController).IsAssignableFrom(i)).ToList();
                        
            var controllerName = base.GetControllerName(request);
            var matchedController = matchedTypes.FirstOrDefault(i => i.Name.ToLower() == controllerName.ToLower() + "controller");
            
            HttpControllerDescriptor http = new HttpControllerDescriptor(_configuration, controllerName, matchedController);

            return http;
        }
    }


       




















    public interface AssembliesResolver
    {
        ICollection<Assembly> GetAssemblies();
    }

    public class MyAssembliesResolver : DefaultAssembliesResolver
    {
        public override ICollection<Assembly> GetAssemblies()
        {
            ICollection<Assembly> baseAssemblies = base.GetAssemblies();
            List<Assembly> assemblies = new List<Assembly>(baseAssemblies);
            var controllersAssembly = Assembly.LoadFrom("c:/myAssymbly.dll");
            baseAssemblies.Add(controllersAssembly);
            return assemblies;
        }
    }

    public class CustomAssemblyResolver : DefaultAssembliesResolver
    {
        public override ICollection<Assembly> GetAssemblies()
        {
            ICollection<Assembly> baseAssemblies = base.GetAssemblies();
            List<Assembly> assemblies = new List<Assembly>(baseAssemblies);

            string thirdPartySource = "C:\\Research\\ExCoWebApi\\ExternalControllers";

            if (!string.IsNullOrWhiteSpace(thirdPartySource))
            {
                if (Directory.Exists(thirdPartySource))
                {
                    foreach (var file in Directory.GetFiles(thirdPartySource, "*.*", SearchOption.AllDirectories))
                    {
                        if (Path.GetExtension(file) == ".dll")
                        {
                            var externalAssembly = Assembly.LoadFrom(file);

                            baseAssemblies.Add(externalAssembly);
                        }
                    }
                }
            }
            return baseAssemblies;
        }
    }

}