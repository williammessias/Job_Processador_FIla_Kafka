using System;
using System.Threading;
using ApresentacaoJobs.DependencyInjection;

namespace ApresentacaoJobs.ProcessarFila
{
    public class ProcessaFila : DependencyResolver
    {
        public ProcessaFila()
        {
            Resolve();
        }

        public void Run()
        {
            while (true)
            {

                service.ProcessarItensFila();

                Thread.Sleep(120000);
            }
        }
    }
}
