using System.Dynamic;
using System.Diagnostics;
using System.Management;

namespace Task.Models
{
    public class Processo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Status { get; set; }
        public string MemoriaFisica { get; set; }
        public string MemoriaPaginada { get; set; }
        public string Usuario { get; set; }
        public string Descricao { get; set; }
        public string TempoTotalProcessador { get; set; }
        public string TempoInicialProcesso { get; set; }
        public double UtilizacaoTotalCPU { get; set; }
        public string Threads { get; set; }
    }
}
