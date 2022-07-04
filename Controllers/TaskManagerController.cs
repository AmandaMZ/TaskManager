using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Diagnostics;
using System.Management;
using Task.Models;

namespace Task.Controllers
{
    public class TaskManagerController : Controller
    {
        public IActionResult Index()
        {
            List<Processo> listaProcessos = GetProcessos();

            ViewBag.Processos           = listaProcessos;
            ViewBag.QuantidadeProcessos = listaProcessos.Count();

            return View();
        }

        public List<Processo> GetProcessos()
        {
            Process[] processList = Process.GetProcesses();

            List<Processo> listaProcessos = new List<Processo>();

            foreach (Process process in processList)
            {
                Processo processo = new Processo();

                var thread1 = new Thread(() =>
                {
                    dynamic extraProcessInfo = GetProcessExtraInformation(process.Id);
                    processo.Usuario   = extraProcessInfo.Username;
                    processo.Descricao = extraProcessInfo.Description;
                });
                thread1.Start();
                
                try
                {
                    string[] processoDetalhes = new string[]
                    {
                        process.StartTime.ToShortTimeString(),
                        process.Threads.Count.ToString(),
                        process.TotalProcessorTime.Duration().Milliseconds.ToString(),
                        process.TotalProcessorTime.Duration().Hours.ToString() + ":" + process.TotalProcessorTime.Duration().Minutes.ToString() + ":" + process.TotalProcessorTime.Duration().Seconds.ToString(),
                    };
                
                    processo.TempoInicialProcesso  = processoDetalhes[0];
                    processo.Threads               = processoDetalhes[1];
                    processo.TempoTotalProcessador = processoDetalhes[3];
                
                    if (process.Id != 0)
                        processo.UtilizacaoTotalCPU = UpdateCpuUsagePercent(Convert.ToDouble(processoDetalhes[2]), process.ProcessName);
                }
                catch { }

                processo.Id              = process.Id;
                processo.Nome            = process.ProcessName;
                processo.Status          = process.Responding == true ? "Em execução" : "Suspenso";
                processo.MemoriaFisica   = GetMemoriaFormatada(process.WorkingSet64);
                processo.MemoriaPaginada = GetMemoriaFormatada(process.PagedMemorySize64);

                //thread1.Join();

                listaProcessos.Add(processo);
            }

            return listaProcessos;
        }

        public string GetMemoriaFormatada(Int64 bytes)
        {
            List<String> listaSufixos = new List<String> { " sBytes", " KB", " MB", " GB", " TB", " PB" };

            int contador = 0;
            decimal valor = (decimal)bytes;
            while (Math.Round(valor / 1024) >= 1)
            {
                valor = valor / 1024;
                contador++;
            }
            return string.Format("{0:n1}{1}", valor, listaSufixos[contador]);
        }

        public ExpandoObject GetProcessExtraInformation(int processId)
        {
            ManagementObjectCollection processList = null;

            var thread2 = new Thread(() =>
            {
                string query = "Select * From Win32_Process Where ProcessID = " + processId;
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                processList = searcher.Get();
            });
            thread2.Start();
        
            dynamic resultado = new ExpandoObject();
            resultado.Description = "";
            resultado.Username    = "";

            thread2.Join();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    resultado.Username = argList[0];
                }
        
                if (obj["ExecutablePath"] != null)
                {
                    try
                    {
                        FileVersionInfo info  = FileVersionInfo.GetVersionInfo(obj["ExecutablePath"].ToString());
                        resultado.Description = info.FileDescription;
                    }
                    catch { }
                }
            }
        
            return resultado;
        }       

        private static double UpdateCpuUsagePercent(double totalMillisegundos, string nome)
        {
            PerformanceCounter totalCpuUsage = new PerformanceCounter("Process", "% Processor Time", nome);
            float utilizacaoTotalCpu = totalCpuUsage.NextValue();

            return totalMillisegundos / (100 - utilizacaoTotalCpu);
        }
    }
}
