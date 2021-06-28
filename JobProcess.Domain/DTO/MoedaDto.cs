using System;
namespace JobProcess.Domain.DTO
{
    public class MoedaCotacao
    {
        public MoedaCotacao(string moeda, DateTime data)
        {
            Moeda = moeda;
            Data = data;
        }
        public string Moeda { get; set; }
        public DateTime Data { get; set; }
    }
}
