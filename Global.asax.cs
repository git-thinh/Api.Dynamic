using System;
using System.ServiceModel.Activation;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Routing;
using System.Web.SessionState;

namespace Api.Dynamic
{
    public class WebApiApplication : System.Web.HttpApplication
    { 
        protected void Application_Start()
        { 
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteTable.Routes.Add(new ServiceRoute("message", new CacheServiceFactory(), typeof(CacheService)));
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            ////Note everything hardcoded, for simplicity!
            //HttpCookie cookie = HttpContext.Current.Request.Cookies.Get("LanguagePref");            
            //HttpContext.Current.Items["__SessionLang"] = language;
        }

        protected void Application_PostAuthorizeRequest()
        {
            //HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            if (context != null && context.Items != null)
            {
                context.Items["socket_message"] = "ws://localhost:56049/message";
            }
        }
    }
}
