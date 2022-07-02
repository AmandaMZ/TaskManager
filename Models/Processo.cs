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
        public string MemoriaFormatada { get; set; }
        public string Usuario { get; set; }
        public string Descricao { get; set; }
        public List<Process> SubProcessos { get; set; }
        public int CPU { get; set; }
        public string TempoTotalProcessador { get; set; }
    }
}
