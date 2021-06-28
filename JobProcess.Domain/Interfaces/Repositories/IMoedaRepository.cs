using System;
using System.Threading.Tasks;
using JobProcess.Domain.DTO;

namespace JobProcess.Domain.Interfaces.Repositories
{
    public interface IMoedaRepository
    {
        DadosMoedaResponse ObterMoedaNaFila();
    }
}
