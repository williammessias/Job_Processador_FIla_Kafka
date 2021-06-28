using System;
namespace JobProcess.Domain.Interfaces.Log
{
    public interface IRepositorioRotinaProcessada
    {
        bool AdicionaLogRotinaProcessada(string rotinaMudancaDeCorretorNaNegociacao);
    }
}
