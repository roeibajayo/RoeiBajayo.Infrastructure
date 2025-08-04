using System.Net;

namespace RoeiBajayo.Infrastructure.Net;

public sealed class IpRange(IPAddress from, IPAddress to)
{
    private readonly System.Net.Sockets.AddressFamily AddressFamily = from.AddressFamily;
    private readonly byte[] FromBytes = from.GetAddressBytes();
    private readonly byte[] ToBytes = to.GetAddressBytes();

    public bool IsInRange(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily)
        {
            return false;
        }

        byte[] addressBytes = address.GetAddressBytes();

        bool lowerBoundary = true, upperBoundary = true;

        for (int i = 0; i < FromBytes.Length &&
            (lowerBoundary || upperBoundary); i++)
        {
            if ((lowerBoundary && addressBytes[i] < FromBytes[i]) ||
                (upperBoundary && addressBytes[i] > ToBytes[i]))
            {
                return false;
            }

            lowerBoundary &= addressBytes[i] == FromBytes[i];
            upperBoundary &= addressBytes[i] == ToBytes[i];
        }

        return true;
    }

}
