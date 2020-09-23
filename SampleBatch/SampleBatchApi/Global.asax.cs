using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace SampleBatchApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static CompositionContainer CompositionContainer
        {
            get;
            set;
        }

        protected void Application_Start()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            DirectoryCatalog directoryCatalog = new DirectoryCatalog(AssemblyDirectory);
            catalog.Catalogs.Add(directoryCatalog);
            CompositionContainer = new CompositionContainer(catalog);
            CompositionContainer.ComposeExportedValue("RedisConnString", ConfigurationManager.AppSettings["RedisConnectionString"]);
            CompositionContainer.ComposeParts(this);

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        private string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}