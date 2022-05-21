using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Helper
{
    public class FileWriterHelper
    {
        public static void WriteLogFile(string text)
        {
            try
            {
                string workingDirectory = Environment.CurrentDirectory;
                string fileName = $"{DateTime.Now.Date}_log.txt";

                if (!Directory.Exists(workingDirectory))
                    Directory.CreateDirectory(workingDirectory);

                StreamWriter sw = new StreamWriter($"{workingDirectory}/{fileName}", true);
                sw.WriteLine(text);
                sw.Flush();
                sw.Close();
            }
            catch(IOException ex)
            {
                string message = ex.Message;
            }
           
            
        }
    }
}
