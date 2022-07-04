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
            List<Processo> listaProcessos = GetProcesso();

            ViewBag.Processos           = listaProcessos;
            ViewBag.QuantidadeProcessos = listaProcessos.Count();

            return View();
        }

        public List<Processo> GetProcesso()
        {
            Process[] processList = Process.GetProcesses();

            List<Processo> processos = new List<Processo>();

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
                        process.TotalProcessorTime.Duration().Hours.ToString() + ":" + process.TotalProcessorTime.Duration().Minutes.ToString() + ":" + process.TotalProcessorTime.Duration().Seconds.ToString()
                    };
                
                    processo.TempoInicialProcesso  = processoDetalhes[0];
                    processo.Threads               = processoDetalhes[1];
                    processo.TempoTotalProcessador = processoDetalhes[3];
                
                    if (process.Id != 0)
                        processo.UtilizacaoTotalCPU = UpdateCpuUsagePercent(Convert.ToDouble(processoDetalhes[2]), process.ProcessName);
                }
                catch { }

                processo.Id               = process.Id;
                processo.Nome             = process.ProcessName;
                processo.Status           = process.Responding == true ? "Em execução" : "Suspenso";
                processo.MemoriaFormatada = GetMemoriaFormatada(process.PrivateMemorySize64);

                thread1.Join();

                processos.Add(processo);
            }

            return processos;
        }

        public string GetMemoriaFormatada(Int64 bytes)
        {
            List<String> listSufixos = new List<String> { " sBytes", " KB", " MB", " GB", " TB", " PB" };

            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, listSufixos[counter]);
        }

        public ExpandoObject GetProcessExtraInformation(int processId)
        {
            ManagementObjectCollection processList = null;

            var thread3 = new Thread(() =>
            {
                string query = "Select * From Win32_Process Where ProcessID = " + processId;
                ManagementObjectSearcher   searcher    = new ManagementObjectSearcher(query);
                processList = searcher.Get();
            });
            thread3.Start();
        
            dynamic response = new ExpandoObject();
            response.Description = "";
            response.Username    = "";

            thread3.Join();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    response.Username = argList[0];
                }
        
                if (obj["ExecutablePath"] != null)
                {
                    try
                    {
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(obj["ExecutablePath"].ToString());
                        response.Description = info.FileDescription;
                    }
                    catch { }
                }
            }
        
            return response;
        }       

        private static double UpdateCpuUsagePercent(double totalMilliseconds, string name)
        {
            PerformanceCounter totalCpuUsage = new PerformanceCounter("Process", "% Processor Time", name);
            float utilizacaoTotalCpu = totalCpuUsage.NextValue();

            return totalMilliseconds / (100 - utilizacaoTotalCpu);
        }
    }
}
