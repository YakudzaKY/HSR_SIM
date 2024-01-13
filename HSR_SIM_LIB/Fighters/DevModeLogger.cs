using System;
using System.IO;
using System.Text;

namespace HSR_SIM_LIB.Fighters
{
    /// <summary>
    /// dev mode logger. Read array from file and write array to file
    /// </summary>
    public class DevModeLogger
    {
        private int[] intArr;
        private string logFilePath;
        private int curPos=0;
        private Worker parentWorker;
        public DevModeLogger(string filePath, Worker parentWorker)
        {
            logFilePath = filePath;
            if (File.Exists(logFilePath))
            {

                string[] lines = File.ReadAllLines(logFilePath);
                intArr = new int[lines.Length];
                int i = 0;
                foreach (string line in lines)
                {
                    intArr[i] = Convert.ToInt32(line);
                    i++;
                }
            }
            else
                intArr = new int[] { };
            this.parentWorker = parentWorker;
        }


        public int ReadNext(string[] arrayStrings, string description)
        {
            
            if (parentWorker.CbGetDecision == null)
                throw new Exception("need set CbGetDecision in dev mode!!");
            curPos++;
            if (curPos > intArr.Length)
            {
                Array.Resize(ref intArr,curPos );
                intArr[curPos-1] = parentWorker.CbGetDecision(arrayStrings,description);
            }
            return intArr[curPos-1];
           
        }

        public void WriteToFile()
        {
            var stringBuilder = new StringBuilder();

            foreach (var arrayElement in intArr)
            {
                stringBuilder.AppendLine(arrayElement.ToString());
            }

            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            File.AppendAllText(logFilePath, stringBuilder.ToString());
        }
    }
}
