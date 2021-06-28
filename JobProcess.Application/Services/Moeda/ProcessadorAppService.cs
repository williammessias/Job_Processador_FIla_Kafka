using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using JobProcess.Application.Interfaces;
using JobProcess.Domain.DTO;
using JobProcess.Domain.Enum;
using JobProcess.Domain.Interfaces.Log;
using JobProcess.Domain.Interfaces.Repositories;

namespace JobProcess.Application.Services
{
    public class ProcessadorAppService : IProcessadorAppService
    {
        private const char DEFAULT_LINE_DELIMITER = ';';

        private readonly IMoedaRepository _repositorioMoeda;
        private readonly ILogProcessamentoRotina _logProcessamento;

        public ProcessadorAppService(IMoedaRepository repositorioMoeda, ILogProcessamentoRotina logProcessamento)
        {
            _repositorioMoeda = repositorioMoeda;
            _logProcessamento = logProcessamento;
        }


        public void ProcessarItensFila()
        {
            var sw = new Stopwatch();

            sw.Start();

            try
            {
                var diretorioArquivo = Directory.GetCurrentDirectory();

                var arquivoDadosMoeda = new StreamReader(File.OpenRead(diretorioArquivo + @"/DadosMoeda.csv"));

                if (arquivoDadosMoeda == null || arquivoDadosMoeda.EndOfStream)
                {
                    sw.Stop();
                    _logProcessamento.AdicionaLogDeProcessamento(DateTime.Now, "Arquivo Dados Moeda não foi encontrado ou está vazio", StatusRotinaEnum.ProcessadaComErro);

                    return;
                }

                var moedasRecuperadasDaFila = _repositorioMoeda.ObterMoedaNaFila();

                if (moedasRecuperadasDaFila == null || !moedasRecuperadasDaFila.DadosMoeda.Any())
                {
                    sw.Stop();
                    _logProcessamento.AdicionaLogDeProcessamento(DateTime.Now, "Não existe nada na fila para processamento", StatusRotinaEnum.NadaParaProcessar);
                    return;
                }

                

                string line;
                DateTime dataMoeda;
                string[]  values;
                List<MoedaCotacao> moedasEncontradas = new List<MoedaCotacao> ();
                List<MoedaDto> moedaNoPeriodo = new List<MoedaDto>();

                while (!arquivoDadosMoeda.EndOfStream && (line = arquivoDadosMoeda.ReadLine()) != null)
                {
                    values = line.Split(DEFAULT_LINE_DELIMITER);

                    if (values[0].Equals("ID_MOEDA"))
                        continue;

                    dataMoeda = Convert.ToDateTime(values[1]);

                    moedaNoPeriodo = moedasRecuperadasDaFila.DadosMoeda.Where(x => x.Moeda == values[0]
                    && (dataMoeda >= x.Data_Inicio && dataMoeda <= x.Data_Fim)).ToList();

                    if (moedaNoPeriodo.Any())
                    {
                        moedasEncontradas.Add(new MoedaCotacao(values[0], dataMoeda));
                    }
                }

                if (!moedasEncontradas.Any())
                {
                    sw.Stop();
                    _logProcessamento.AdicionaLogDeProcessamento(DateTime.Now, "Não foi encontrada nenhuma moeda na plailha csv para os periodos encontrados", StatusRotinaEnum.NadaParaProcessar, sw.Elapsed.TotalSeconds);
                    return;
                }

                var deParaMoedaCodCotacao = RecuperarValoresDeParaCotacoes();

                var moedaDeParaCotacao = from moedas in moedasEncontradas
                                         join dePara in deParaMoedaCodCotacao
                         on moedas.Moeda equals dePara.Moeda
                                         select new { dePara.Moeda, moedas.Data, dePara.CodCotacao};

                var t = moedaDeParaCotacao.ToList();
                if (!moedaDeParaCotacao.Any())
                {
                    sw.Stop();
                    _logProcessamento.AdicionaLogDeProcessamento(DateTime.Now, "Não foi encontrada nenhuma moeda no dePara", StatusRotinaEnum.NadaParaProcessar, sw.Elapsed.TotalSeconds);

                }

            

                List<DadosCotacaoParaCsv> cotacoesEncontradas = new List<DadosCotacaoParaCsv>();
                CultureInfo ptBR = new CultureInfo("pt-BR");

                var valorComissao = "";

                
                var arquivoDadosCotacao = new StreamReader(File.OpenRead(diretorioArquivo + @"/DadosCotacao.csv"));
                if (arquivoDadosCotacao == null || arquivoDadosCotacao.EndOfStream)
                {
                    sw.Stop();
                    _logProcessamento.AdicionaLogDeProcessamento(DateTime.Now, "Arquivo Dados Cotacao não foi encontrado ou está vazio", StatusRotinaEnum.ProcessadaComErro, sw.Elapsed.TotalSeconds);
                }


                var listaMoedas = moedaDeParaCotacao.Select(x => x.CodCotacao).Distinct().ToList();

                while ((line = arquivoDadosCotacao.ReadLine()) != null)
                {
                    foreach (var codigoCotacao in listaMoedas)
                    {

                        var lookingFor = codigoCotacao.AsSpan();

                        var span = line.AsSpan(line.IndexOf(';') + 1);


                        var firstPos = span.IndexOf(';');
                        var codCotacao = span.Slice(0, firstPos);
                        var codCotacaoParametro = codCotacao.ToString() ;

                        if (!codCotacao.SequenceEqual(lookingFor)) continue;

                        span = span.Slice(firstPos + 1);

                        var dataEncontradaMoeda = Convert.ToDateTime(span.ToString()).ToString("dd/MM/yyyy");

                        var moeda = moedaDeParaCotacao.Where(x => Convert.ToDateTime(x.Data.ToString()).ToString("dd/MM/yyyy") == dataEncontradaMoeda
                                                    && x.CodCotacao == Convert.ToString(codCotacaoParametro)).Select(y=>y.Moeda).FirstOrDefault();

                        if (string.IsNullOrEmpty(moeda))
                            continue;

                        valorComissao = line.Split(';').FirstOrDefault();


                        cotacoesEncontradas.Add(new DadosCotacaoParaCsv(moeda, dataEncontradaMoeda, valorComissao));
                    }


                }

                if (!cotacoesEncontradas.Any())
                {
                    sw.Stop();
                    _logProcessamento.AdicionaLogDeProcessamento(DateTime.Now, "Não foram encontradas cotações para os periodos informados", StatusRotinaEnum.NadaParaProcessar, sw.Elapsed.TotalSeconds);

                }

                GerarArquivoCSV(cotacoesEncontradas);

                sw.Stop();
                _logProcessamento.AdicionaLogDeProcessamento(DateTime.Now, "Processada com sucesso", StatusRotinaEnum.ProcessadaComSucesso, sw.Elapsed.TotalSeconds);

            }
            catch (Exception ex)
            {
                sw.Stop();
                _logProcessamento.AdicionaLogDeProcessamento(DateTime.Now, "Erro no processamento - Arquivos não encontrados na pasta" + ex.Message, StatusRotinaEnum.ProcessadaComErro, sw.Elapsed.TotalSeconds);
            }
        }

        private void GerarArquivoCSV(List<DadosCotacaoParaCsv> cotacoesEncontradas)
        {
            var fileName = "Resultado_"+DateTime.Now.ToString("yyyy/MM/dd HH:mm/ss").Replace("/","").Replace(" ","_").Replace(":", "");

            var sb = new StringBuilder();
            var basePath = Directory.GetCurrentDirectory();
            var finalPath = Path.Combine(basePath, fileName + ".csv");
            var header = "";
            var info = typeof(DadosCotacao).GetProperties();
            if (!File.Exists(finalPath))
            {
                var file = File.Create(finalPath);
                file.Close();

                header = "ID_MOEDA;DATA_REF;VL_COTACAO" +"; ";

                header = header.Substring(0, header.Length - 2);
                sb.AppendLine(header);
                TextWriter sw = new StreamWriter(finalPath, true);
                sw.Write(sb.ToString());
                sw.Close();
            }
            foreach (var obj in cotacoesEncontradas)
            {
                sb = new StringBuilder();
                var line = "";
                line = obj.ID_MOEDA + "; " + obj.DATA_REF + "; " + obj.VL_COTACAO + "; ";

                line = line.Substring(0, line.Length - 2);
                sb.AppendLine(line);
                TextWriter sw = new StreamWriter(finalPath, true);
                sw.Write(sb.ToString());
                sw.Close();
            }
        }

        private List<DadosCotacao> RecuperarValoresDeParaCotacoes()
        {
            var dadosCotacao = new List<DadosCotacao>();

            dadosCotacao.Add(new DadosCotacao("AFN", "66"));
            dadosCotacao.Add(new DadosCotacao("ALL","49"));
            dadosCotacao.Add(new DadosCotacao("ANG","33"));
            dadosCotacao.Add(new DadosCotacao("ARS","3 "));
            dadosCotacao.Add(new DadosCotacao("AWG","6 "));
            dadosCotacao.Add(new DadosCotacao("BOB","56"));
            dadosCotacao.Add(new DadosCotacao("BYN","64"));
            dadosCotacao.Add(new DadosCotacao("CAD","25"));
            dadosCotacao.Add(new DadosCotacao("CDF","58"));
            dadosCotacao.Add(new DadosCotacao("CLP","16"));
            dadosCotacao.Add(new DadosCotacao("COP","37"));
            dadosCotacao.Add(new DadosCotacao("CRC","52"));
            dadosCotacao.Add(new DadosCotacao("CUP","8 "));
            dadosCotacao.Add(new DadosCotacao("CVE","51"));
            dadosCotacao.Add(new DadosCotacao("CZK","29"));
            dadosCotacao.Add(new DadosCotacao("DJF","36"));
            dadosCotacao.Add(new DadosCotacao("DZD","54"));
            dadosCotacao.Add(new DadosCotacao("EGP","12"));
            dadosCotacao.Add(new DadosCotacao("EUR","20"));
            dadosCotacao.Add(new DadosCotacao("FJD","38"));
            dadosCotacao.Add(new DadosCotacao("GBP","22"));
            dadosCotacao.Add(new DadosCotacao("GEL","48"));
            dadosCotacao.Add(new DadosCotacao("GIP","18"));
            dadosCotacao.Add(new DadosCotacao("HTG","63"));
            dadosCotacao.Add(new DadosCotacao("ILS","40"));
            dadosCotacao.Add(new DadosCotacao("IRR","17"));
            dadosCotacao.Add(new DadosCotacao("ISK","11"));
            dadosCotacao.Add(new DadosCotacao("JPY","9 "));
            dadosCotacao.Add(new DadosCotacao("KES","21"));
            dadosCotacao.Add(new DadosCotacao("KMF","19"));
            dadosCotacao.Add(new DadosCotacao("LBP","42"));
            dadosCotacao.Add(new DadosCotacao("LSL","4 "));
            dadosCotacao.Add(new DadosCotacao("MGA","35"));
            dadosCotacao.Add(new DadosCotacao("MGB","26"));
            dadosCotacao.Add(new DadosCotacao("MMK","69"));
            dadosCotacao.Add(new DadosCotacao("MRO","53"));
            dadosCotacao.Add(new DadosCotacao("MRU","15"));
            dadosCotacao.Add(new DadosCotacao("MUR","7 "));
            dadosCotacao.Add(new DadosCotacao("MXN","41"));
            dadosCotacao.Add(new DadosCotacao("MZN","43"));
            dadosCotacao.Add(new DadosCotacao("NIO","23"));
            dadosCotacao.Add(new DadosCotacao("NOK","62"));
            dadosCotacao.Add(new DadosCotacao("OMR","34"));
            dadosCotacao.Add(new DadosCotacao("PEN","45"));
            dadosCotacao.Add(new DadosCotacao("PGK","2 "));
            dadosCotacao.Add(new DadosCotacao("PHP","24"));
            dadosCotacao.Add(new DadosCotacao("RON","5 "));
            dadosCotacao.Add(new DadosCotacao("SAR","44"));
            dadosCotacao.Add(new DadosCotacao("SBD","32"));
            dadosCotacao.Add(new DadosCotacao("SGD","70"));
            dadosCotacao.Add(new DadosCotacao("SLL","10"));
            dadosCotacao.Add(new DadosCotacao("SOS","61"));
            dadosCotacao.Add(new DadosCotacao("SSP","47"));
            dadosCotacao.Add(new DadosCotacao("SZL","55"));
            dadosCotacao.Add(new DadosCotacao("THB","39"));
            dadosCotacao.Add(new DadosCotacao("TRY","13"));
            dadosCotacao.Add(new DadosCotacao("TTD","67"));
            dadosCotacao.Add(new DadosCotacao("UGX","59"));
            dadosCotacao.Add(new DadosCotacao("USD","1 "));
            dadosCotacao.Add(new DadosCotacao("UYU","46"));
            dadosCotacao.Add(new DadosCotacao("VES","68"));
            dadosCotacao.Add(new DadosCotacao("VUV","57"));
            dadosCotacao.Add(new DadosCotacao("WST","28"));
            dadosCotacao.Add(new DadosCotacao("XAF","30"));
            dadosCotacao.Add(new DadosCotacao("XAU","60"));
            dadosCotacao.Add(new DadosCotacao("XDR","27"));
            dadosCotacao.Add(new DadosCotacao("XOF","14"));
            dadosCotacao.Add(new DadosCotacao("XPF","50"));
            dadosCotacao.Add(new DadosCotacao("ZAR","65"));
            dadosCotacao.Add(new DadosCotacao("ZWL", "31"));

            return dadosCotacao;
        }
    }
}
