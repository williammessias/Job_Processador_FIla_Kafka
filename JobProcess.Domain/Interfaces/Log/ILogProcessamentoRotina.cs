using System;
using JobProcess.Domain.Enum;

namespace JobProcess.Domain.Interfaces.Log
{
    public interface ILogProcessamentoRotina
    {
        bool AdicionaLogDeProcessamento(DateTime inicioProcessamento, string mensagemErro, StatusRotinaEnum statusProcessamento, double? tempoGastoProcessamento = null);
    }
}
