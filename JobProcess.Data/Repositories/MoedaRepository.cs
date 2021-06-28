using System;
using System.IO;
using System.Net;
using JobProcess.Domain.DTO;
using JobProcess.Domain.Interfaces.Repositories;
using Newtonsoft.Json;

namespace JobProcess.Repositories.Data
{
    public class MoedaRepository : IMoedaRepository
    {
        public  DadosMoedaResponse ObterMoedaNaFila()
        {
            var url = "http://localhost:5000/v1/Cotacao/GetItemFila";
            return EnviarRequisicaoGet(url);
        }

        private DadosMoedaResponse EnviarRequisicaoGet(string url)
        {
            var requisicaoWeb = WebRequest.CreateHttp(url);
            requisicaoWeb.Method = "GET";
            using (var resposta = requisicaoWeb.GetResponse())
            {
                var streamDados = resposta.GetResponseStream();
                StreamReader reader = new StreamReader(streamDados);
                object objResponse = reader.ReadToEnd();
                var dadosMoeda = JsonConvert.DeserializeObject<DadosMoedaResponse>(objResponse.ToString());
                streamDados.Close();
                resposta.Close();

                return dadosMoeda;
            }
        }
    }
}
