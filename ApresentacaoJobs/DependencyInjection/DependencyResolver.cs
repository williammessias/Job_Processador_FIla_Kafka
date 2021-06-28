using System;
using JobProcess.Application.Interfaces;
using JobProcess.Application.Services;
using JobProcess.Data.Log;
using JobProcess.Domain.Interfaces.Log;
using JobProcess.Domain.Interfaces.Repositories;
using JobProcess.Repositories.Data;
using Unity;

namespace ApresentacaoJobs.DependencyInjection
{
    public class DependencyResolver
    {
        private readonly UnityContainer container = new UnityContainer();

        public ProcessadorAppService service { get; private set; }

        public void Resolve()
        {
            container.RegisterType<IProcessadorAppService, ProcessadorAppService>();
            container.RegisterType<IMoedaRepository, MoedaRepository>();
            container.RegisterType<ILogProcessamentoRotina, LogProcessamento>();

            service = container.Resolve<ProcessadorAppService>();
        }

    }
}
