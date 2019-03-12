using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Moggles.Tests
{
    public class TestFixture<TStartup> : IDisposable
    {
        private TestServer _server;
        private readonly bool _reflectBaseType;

        public HttpClient Client { get; set; }

        public TestFixture(string relativeTargetProjectParentDir, bool reflectBaseType)
        {
            this._reflectBaseType = reflectBaseType;
            this.BootstrapServerAndClient(relativeTargetProjectParentDir);
        }

        private void BootstrapServerAndClient(string relativeTargetProjectParentDir)
        {
            Assembly startupAssembly = this._reflectBaseType ? ((Type)IntrospectionExtensions.GetTypeInfo(typeof(TStartup))).BaseType.Assembly : ((Type)IntrospectionExtensions.GetTypeInfo(typeof(TStartup))).Assembly;
            this._server = new TestServer(ApplicationInsightsWebHostBuilderExtensions.UseApplicationInsights((IWebHostBuilder)new WebHostBuilder()).UseContentRoot(TestFixture<TStartup>.GetProjectPath(relativeTargetProjectParentDir, startupAssembly)).ConfigureServices(new Action<IServiceCollection>(this.InitializeServices)).UseEnvironment("Testing").UseStartup(typeof(TStartup)));
            this.Client = this._server.CreateClient();
            this.Client.BaseAddress = new Uri("http://localhost");
        }

        public void Dispose()
        {
            ((HttpMessageInvoker)this.Client).Dispose();
            this._server.Dispose();
        }

        protected virtual void InitializeServices(IServiceCollection services)
        {
            Assembly assembly = this._reflectBaseType ? ((Type)IntrospectionExtensions.GetTypeInfo(typeof(TStartup))).BaseType.Assembly : ((Type)IntrospectionExtensions.GetTypeInfo(typeof(TStartup))).Assembly;
            ApplicationPartManager implementationInstance = new ApplicationPartManager();
            implementationInstance.ApplicationParts.Add((ApplicationPart)new AssemblyPart(assembly));
            implementationInstance.FeatureProviders.Add((IApplicationFeatureProvider)new ControllerFeatureProvider());
            implementationInstance.FeatureProviders.Add((IApplicationFeatureProvider)new ViewComponentFeatureProvider());
            services.AddSingleton<ApplicationPartManager>(implementationInstance);
        }

        private static string GetProjectPath(string projectRelativePath, Assembly startupAssembly)
        {
            string name = startupAssembly.GetName().Name;
            string baseDirectory = AppContext.BaseDirectory;
            DirectoryInfo directoryInfo1 = new DirectoryInfo(baseDirectory);
            do
            {
                directoryInfo1 = directoryInfo1.Parent;
                DirectoryInfo directoryInfo2 = new DirectoryInfo(Path.Combine(directoryInfo1.FullName, projectRelativePath));
                if (directoryInfo2.Exists && new FileInfo(Path.Combine(directoryInfo2.FullName, name, string.Format("{0}.csproj", (object)name))).Exists)
                    return Path.Combine(directoryInfo2.FullName, name);
            }
            while (directoryInfo1.Parent != null);
            throw new Exception(string.Format("Project root could not be located using the application root {0}.", (object)baseDirectory));
        }
    }
}