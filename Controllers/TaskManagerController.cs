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
                processo.Id     = process.Id;
                processo.Nome   = process.ProcessName;
                processo.Status = process.Responding == true ? "Em execução" : "Suspenso";

                dynamic extraProcessInfo = GetProcessExtraInformation(process.Id);
                processo.Usuario   = extraProcessInfo.Username;
                processo.Descricao = extraProcessInfo.Description;

                processo.MemoriaFormatada = GetMemoriaFormatada(process.PrivateMemorySize64);
                
                processo.SubProcessos     = childProcess;

                //PerformanceCounter performanceCounter = new PerformanceCounter("Process", "ID Process", process.ProcessName, true);
                //processo.CPU = performanceCounter.NextValue() + "%";

                //if (process.Id != 0)
                //{
                //    TimeSpan t = process.TotalProcessorTime;
                //    processo.TempoTotalProcessador = t.ToString().Substring(0,8);
                //    long l = 0;
                //}

                //PerformanceCounter total_cpu   = new PerformanceCounter("Process", "% Processor Time", "_Total");
                //PerformanceCounter process_cpu = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                
                //if (process_cpu.NextValue() && total_cpu.NextValue())
                //    String s = (process_cpu.NextValue() / total_cpu.NextValue() * 100) + "%";
                
                //processo.CPU = GetCpuUsage(process.ProcessName);

                processos.Add(processo);            
            }

            return processos;
        }

        //public int GetCpuUsage(string nome)
        //{
        //    PerformanceCounter cpuCounter;
        //    cpuCounter = new PerformanceCounter("Process", "% Processor Time", nome);
        //    var a = cpuCounter.NextValue();
        //
        //    return (int)cpuCounter.NextValue();
        //}

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
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            dynamic response = new ExpandoObject();
            response.Description = "";
            response.Username = "";

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

        //public int? GetParentId(Process process)
        //{
        //    string queryText = string.Format("select parentprocessid, name from win32_process where processid = {0}", process.Id);
        //    using (var searcher = new ManagementObjectSearcher(queryText))
        //    {
        //        foreach (var obj in searcher.Get())
        //        {
        //            object data = obj.Properties["parentprocessid"].Value;
        //            object name = obj.Properties["name"].Value;

        //            if (data != null)
        //                return Convert.ToInt32(data);
        //        }
        //    }
        //    return null;
        //}
    }
}
