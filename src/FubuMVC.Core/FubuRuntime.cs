using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Bottles;
using Bottles.Diagnostics;
using FubuMVC.Core.Bootstrapping;
using FubuMVC.Core.Runtime;

namespace FubuMVC.Core
{
    /// <summary>
    /// Represents a running FubuMVC application, with access to the key parts of the application
    /// </summary>
    public class FubuRuntime : IDisposable
    {
        private readonly IContainerFacility _facility;
        private readonly IServiceFactory _factory;
        private readonly IList<RouteBase> _routes;
        private bool _disposed;

        public FubuRuntime(IServiceFactory factory, IContainerFacility facility, IList<RouteBase> routes)
        {
            _factory = factory;
            _facility = facility;
            _routes = routes;
        }

        public IServiceFactory Factory
        {
            get { return _factory; }
        }

        public IContainerFacility Facility
        {
            get { return _facility; }
        }

        public IList<RouteBase> Routes
        {
            get { return _routes; }
        }

        public void Dispose()
        {
            dispose();
            GC.SuppressFinalize(this);
        }

        private void dispose()
        {
             if (_disposed) return;

            _disposed = true;

            var deactivators = _factory.GetAll<IDeactivator>().ToArray();
            var log = new PackageLog();
            
            deactivators.Each(x => {
                try
                {
                    log.Trace("Running " + x);
                    x.Deactivate(log);
                }
                catch (Exception e)
                {
                    log.MarkFailure(e);
                }
            });

            Facility.Shutdown();

            Console.WriteLine(log.FullTraceText());
        }

        ~FubuRuntime()
        {
            try
            {
                dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred in the finalizer {0}", ex);
            }
        }
    }
}