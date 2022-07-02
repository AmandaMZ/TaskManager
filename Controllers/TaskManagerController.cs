using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Diagnostics;
using System.Management;
using Task.Models;


namespace Task.Controllers
{
    public class TaskManagerController : Controller
    {
        private string nomeProcesso = "";

        public IActionResult Index()
        {                     
            ViewBag.Processos = GetProcesso();

            return View();
        }

        public List<Processo> GetProcesso()
        {            
            Process[] processList = Process.GetProcesses();

            List<Processo> processos    = new List<Processo>();
            List<Process>  childProcess = new List<Process> ();

            foreach (Process process in processList)
            {
                // childProcess = GetChildProcesses(process.Id);

                Processo processo = new Processo();
                processo.Id               = process.Id;
                processo.Nome             = process.ProcessName;
                processo.Status           = process.Responding == true ? "Em execução" : "Suspenso";
                processo.MemoriaFormatada = GetMemoriaFormatada(process.PrivateMemorySize64);
                processo.SubProcessos     = childProcess;
                
                dynamic extraProcessInfo = GetProcessExtraInformation(process.Id);
                processo.Usuario   = extraProcessInfo.Username;
                processo.Descricao = extraProcessInfo.Description;
                
                try
                {                 
                    string[] processoDetalhes = new string[] { process.StartTime.ToShortTimeString(), process.TotalProcessorTime.Duration().Hours.ToString() + ":" + process.TotalProcessorTime.Duration().Minutes.ToString() + ":" + process.TotalProcessorTime.Duration().Seconds.ToString(), (process.WorkingSet64 / 1024).ToString() + "k", (process.PeakWorkingSet64 / 1024).ToString() + "k", process.HandleCount.ToString(), process.Threads.Count.ToString(), process.TotalProcessorTime.Duration().Milliseconds.ToString() };

                    processo.TempoInicialProcesso  = processoDetalhes[0];
                    processo.TempoTotalProcessador = processoDetalhes[1];

                    if (process.Id != 0)
                    {
                        processo.UtilizacaoTotalCpu = UpdateCpuUsagePercent(Convert.ToDouble(processoDetalhes[6]), process.ProcessName);
                    }
                }
                catch { }

                processos.Add(processo);            
            }

            return processos;
        }

       
        public string GetMemoriaFormatada(Int64 bytes)
        {
            List<String> listSufixos = new List<String> { "Bytes", "KB", "MB", "GB", "TB", "PB" };

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
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher   searcher    = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();
        
            dynamic response = new ExpandoObject();
            response.Description = "";
            response.Username    = "";
        
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

        public List<Process> GetChildProcesses(int processId)
        {
            var results = new List<Process>();

            string queryText = string.Format("select processid from win32_process where parentprocessid = {0}", processId);
            using (var searcher = new ManagementObjectSearcher(queryText))
            {
                foreach (var obj in searcher.Get())
                {
                    results.Add(Process.GetProcessById(Convert.ToInt32(obj.Properties["processid"].Value)));
                }
            }

            return results;
        }

        private static PerformanceCounter TotalCpuUsage()
        {
            return new PerformanceCounter("Process", "% Processor Time", "Idle");
        }

        private static double UpdateCpuUsagePercent(double totalMilliseconds, string name)
        {
            double Total = 0;

            PerformanceCounter totalCpuUsage = new PerformanceCounter("Process", "% Processor Time", name);
            float utilizacaoTotalCpu = totalCpuUsage.NextValue();

            return totalMilliseconds / (100 - utilizacaoTotalCpu);
        }
    }
}
