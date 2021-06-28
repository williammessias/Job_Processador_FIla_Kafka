using System;
using System.IO;
using System.Reflection;
using JobProcess.Domain.Enum;
using JobProcess.Domain.Interfaces.Log;

namespace JobProcess.Data.Log
{
    public class LogProcessamento : ILogProcessamentoRotina
    {
        private static string caminhoExe = string.Empty;

        public bool AdicionaLogDeProcessamento(DateTime inicioProcessamento, string mensagemErro, StatusRotinaEnum statusProcessamento, double? tempoGastoProcessamento = null)
        {
            try
            {
                caminhoExe = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string caminhoArquivo = Path.Combine(caminhoExe, "ArquivoLog.txt");
                if (!File.Exists(caminhoArquivo))
                {
                    FileStream arquivo = File.Create(caminhoArquivo);
                    arquivo.Close();
                }

                string strMensagem = "Inicio do processamento: "+ inicioProcessamento +" mensagem: "+ mensagemErro +" Status de finalização da rotina: "+ statusProcessamento.ToString()+ " tempo gasto no processamento: "+ tempoGastoProcessamento ;

                using (StreamWriter w = File.AppendText(caminhoArquivo))
                {
                    AppendLog(strMensagem, w);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }


        private static void AppendLog(string logMensagem, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entrada : ");
                txtWriter.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine($"  :{logMensagem}");
                txtWriter.WriteLine("------------------------------------");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }


}
