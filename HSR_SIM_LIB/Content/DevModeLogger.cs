using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace HSR_SIM_LIB.Fighters;

/// <summary>
///     dev mode logger. Read array from file and write array to file
/// </summary>
public class DevModeLogger
{
    private int curPos;
    private int[] intArr;
    private readonly string logFilePath;
    private readonly Worker parentWorker;

    public DevModeLogger(string filePath, Worker parentWorker)
    {
        logFilePath = filePath;
        if (File.Exists(logFilePath))
        {
            var lines = File.ReadAllLines(logFilePath);
            intArr = new int[lines.Length];
            var i = 0;
            foreach (var line in lines)
            {
                intArr[i] = Convert.ToInt32(line);
                i++;
            }
        }
        else
        {
            intArr = new int[] { };
        }

        this.parentWorker = parentWorker;
    }

    /// <summary>
    ///     read next value from log or ask user if no record exists
    /// </summary>
    /// <param name="arrayStrings">items to select</param>
    /// <param name="description">description show to user</param>
    /// <returns>index selected</returns>
    /// <exception cref="Exception"></exception>
    public int ReadNext(string[] arrayStrings, string description)
    {
        if (parentWorker.CbGetDecision == null)
            throw new Exception("need set CbGetDecision in dev mode!!");
        curPos++;
        if (curPos > intArr.Length)
        {
            Array.Resize(ref intArr, curPos);
            intArr[curPos - 1] = parentWorker.CbGetDecision(arrayStrings, description);
        }

        return intArr[curPos - 1];
    }

    public void WriteToFile()
    {
        var stringBuilder = new StringBuilder();

        foreach (var arrayElement in intArr) stringBuilder.AppendLine(arrayElement.ToString());

        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
        File.WriteAllText(logFilePath, stringBuilder.ToString());
    }


    public void WriteResToFile()
    {
        var res = parentWorker.GetCombatResult();
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
        File.WriteAllText(logFilePath + "_res.json", JsonConvert.SerializeObject(res));
    }
}