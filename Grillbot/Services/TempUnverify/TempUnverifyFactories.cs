using System;

namespace Grillbot.Services.TempUnverify
{
    public class TempUnverifyFactories
    {
        private IServiceProvider Provider { get; }

        public TempUnverifyFactories(IServiceProvider provider)
        {
            Provider = provider;
        }

        private TService GetService<TService>() => (TService)Provider.GetService(typeof(TService));

        public TempUnverifyChecker GetChecker() => GetService<TempUnverifyChecker>();
        public TempUnverifyHelper GetHelper() => GetService<TempUnverifyHelper>();
    }
}
