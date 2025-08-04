using System;
using System.IO;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Processes;

public static class NodeJsProcess
{
    public static string StreamFile(string nodeFile, Stream content, string? arguments = null) =>
        WriteAsBase64(nodeFile, content.ReadAllBytes(), arguments);
    public static async Task<string> StreamFileAsync(string nodeFile, Stream content, string? arguments = null) =>
        WriteAsBase64(nodeFile, await content.ReadAllBytesAsync(), arguments);
    public static string WriteAsBase64(string nodeFile, byte[] bytes, string? arguments = null)
    {
        var workingDirectory = Path.GetDirectoryName(nodeFile)!;
        return ProcessHelper.ReadProcess(Convert.ToBase64String(bytes),
            "node", nodeFile + " " + arguments, workingDirectory);
    }
}
