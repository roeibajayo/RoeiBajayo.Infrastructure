namespace Infrastructure.Utils.Net.Models;

public class GeoLocationResult
{
    private const string ISRAEL_COUNTRY_CODE = "IL";

    public required string Country { get; set; }
    public required string Region { get; set; }
    public required string City { get; set; }
    public required string Timezone { get; set; }

    public bool IsIsrael() => Country == ISRAEL_COUNTRY_CODE;
}