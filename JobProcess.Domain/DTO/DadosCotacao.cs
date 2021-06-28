using System;
namespace JobProcess.Domain.DTO
{
    public class DadosCotacao
    {

        public DadosCotacao(string moeda, string codCotacao)
        {
            Moeda = moeda;
            CodCotacao = codCotacao;
        }



        public string Moeda { get; set; }
        public string CodCotacao { get; set; }

    }

    public class DadosCotacaoParaCsv
    {

        public DadosCotacaoParaCsv(string moeda, string dataCotacao, string valorCotacao)
        {

            ID_MOEDA = moeda;
            DATA_REF = dataCotacao;
            VL_COTACAO = valorCotacao;

        }

        public string ID_MOEDA { get; set; }
        public string DATA_REF { get; set; }
        public string VL_COTACAO { get; set; }
    }
}


    

