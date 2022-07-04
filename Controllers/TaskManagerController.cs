using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Diagnostics;
using System.Management;
using Task.Models;
using System.Collections.Generic;

namespace Task.Controllers
{
    public class TaskManagerController : Controller
    {
        Dictionary<int, Processo> dictProcessos = new Dictionary<int, Processo>();

        public IActionResult Index()
        {
            GetProcessoEstatico(dictProcessos);
            GetProcesso(dictProcessos);

            ViewBag.Processos           = dictProcessos;
            ViewBag.QuantidadeProcessos = dictProcessos.Count();

            return View();
        }

        public void GetProcessoEstatico(Dictionary<int, Processo> dictProcessos)
        {            
            Process[] processList = Process.GetProcesses();

            foreach (Process process in processList)
            {
                Processo processo = new Processo();

                var thread1 = new Thread(() =>
                {
                    dynamic extraProcessInfo = GetProcessExtraInformation(process.Id);
                    processo.Usuario = extraProcessInfo.Username;
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
                    };
        
                    processo.TempoInicialProcesso = processoDetalhes[0];
                    processo.Threads              = processoDetalhes[1];
        
                    if (process.Id != 0)
                        processo.UtilizacaoTotalCPU = UpdateCpuUsagePercent(Convert.ToDouble(processoDetalhes[2]), process.ProcessName);
                }
                catch { }

                processo.Id   = process.Id;
                processo.Nome = process.ProcessName;

                thread1.Join();

                dictProcessos[process.Id] = processo;
            }
        }

        public void GetProcesso(Dictionary<int, Processo> dictProcessos)
        {
            Process[] processList = Process.GetProcesses();

            foreach (Process process in processList)
            {
                var filter = dictProcessos.FirstOrDefault(p => p.Key == process.Id);

                if (filter.Value == null)
                {
                    dictProcessos.Remove(process.Id);
                    continue;
                }

                Processo processo = filter.Value;

                try
                {
                    string[] processoDetalhes = new string[]
                    {
                        process.TotalProcessorTime.Duration().Hours.ToString() + ":" + process.TotalProcessorTime.Duration().Minutes.ToString() + ":" + process.TotalProcessorTime.Duration().Seconds.ToString()
                    };

                    processo.TempoTotalProcessador = processoDetalhes[0];
                }
                catch { }

                processo.Status           = process.Responding == true ? "Em execução" : "Suspenso";
                processo.MemoriaFormatada = GetMemoriaFormatada(process.PrivateMemorySize64);
            }
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
