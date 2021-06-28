using System;
using ApresentacaoJobs.ProcessarFila;

namespace ApresentacaoJobs
{
    public class MainClass  
    {
        private static ProcessaFila _controller = new ProcessaFila();

        public static void Main()
        {
            _controller.Run();
        }
    }
}
