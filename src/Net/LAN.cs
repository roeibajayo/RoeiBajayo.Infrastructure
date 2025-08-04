using Infrastructure.Utils.Http;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Infrastructure.Utils.Net;

public class LAN(IRestClient client)
{
    private static readonly string[] PublicIpUrls = [
        "http://ipinfo.io/ip",
        "http://icanhazip.com/",
        "http://ipecho.net/plain",
        "http://testp1.piwo.pila.pl/testproxy.php",
        "http://bot.whatismyipaddress.com"
    ];

    public static IPAddress GetLocalIPAddress()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList
            .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
    }

    public static bool IsPortAvailable(int port)
    {
        return !IPGlobalProperties.GetIPGlobalProperties()
            .GetActiveTcpConnections()
            .Any(x => x.LocalEndPoint.Port == port);
    }

    public static int GetFreeTCPPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var freeport = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return freeport;
    }

    public async Task<string?> GetPublicIPAsync()
    {
        foreach (var url in PublicIpUrls)
        {
            try
            {
                var ip = await client.GetAsync<string>(url);
                return ip!.Trim();
            }
            catch { }
        }
        return null;
    }
}
