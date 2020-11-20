using Microsoft.Extensions.DependencyInjection;
using System;

namespace Grillbot.Models
{
    public class ScopedService<TService> : IDisposable
    {
        public TService Service { get; }
        public IServiceScope Scope { get; }

        public ScopedService(TService service, IServiceScope scope)
        {
            Service = service;
            Scope = scope;
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}
