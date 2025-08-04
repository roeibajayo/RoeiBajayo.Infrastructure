using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Infrastructure.Utils.Processes;

public static class ProcessHelper
{
    public static string ReadProcess(string write,
        string fileName,
        string? arguments = null,
        string? workingDirectory = null)
    {
        var processInfo = new ProcessStartInfo(fileName, arguments!)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            ErrorDialog = false,
            WorkingDirectory = workingDirectory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = Encoding.ASCII,
            StandardOutputEncoding = Encoding.UTF8
        };

        using var process = new Process();
        process.StartInfo = processInfo;
        process.Start();

        foreach (var chunk in write.Chunks(500))
            process.StandardInput.Write(chunk);

        process.StandardInput.Flush();
        process.StandardInput.Close();

        return process.StandardOutput.ReadToEnd();
    }
}
