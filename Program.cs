using RestSharp;
using System.Text.Json;

public class OauthToken
{
    public int status_code { get; set; }
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string userId { get; set; }
    public string scope { get; set; }
}

public class QuotaStatus
{
    public int id { get; set; }
    public string description { get; set; }
}

public class QuotaStatusSMS
{
    public int id { get; set; }
    public string description { get; set; }
}

public class SimCard
{
    public string iccid { get; set; }
    public string imsi { get; set; }
    public string msisdn { get; set; }
    public string imei { get; set; }
    public bool imei_lock { get; set; }
    public string status { get; set; }
    public string activation_date { get; set; }
    public string ip_address { get; set; }
    public int current_quota { get; set; }
    public QuotaStatus quota_status { get; set; }
    public int current_quota_SMS { get; set; }
    public QuotaStatusSMS quota_status_SMS { get; set; }
}

internal class Program
{
    private static readonly string BasicToken = "";
    public static string? GetAcessToken()
    {
        var client = new RestClient("https://api.1nce.com/management-api/oauth/token");
        var request = new RestRequest
        {
            Method = Method.Post
        };
        request.AddHeader("accept", "application/json");
        request.AddHeader("content-type", "application/json");
        request.AddHeader("authorization", $"Basic {BasicToken}");
        request.AddParameter("application/json", "{\"grant_type\":\"client_credentials\"}", ParameterType.RequestBody);
        RestResponse response = client.Execute(request);

        if (response.Content is null) return null;

        OauthToken? oauthToken = JsonSerializer.Deserialize<OauthToken>(response.Content);

        return oauthToken?.access_token;
    }

    private static string ActivateIccID(string accessToken, string iccid)
    {
        var client = new RestClient("https://api.1nce.com/management-api/v1/sims");
        var request = new RestRequest
        {
            Method = Method.Post
        };
        request.AddHeader("accept", "application/json");
        request.AddHeader("content-type", "application/json");
        request.AddHeader("authorization", $"Bearer {accessToken}");
        request.AddParameter("application/json", $"[{{\"imei_lock\":false,\"status\":\"Enabled\",\"iccid\":\"{iccid}\"}}]", ParameterType.RequestBody);
        RestResponse response = client.Execute(request);
        //response.StatusCode should be Created
        return response.StatusCode.ToString();
    }

    private static string? GetIccIdStatus(string accessToken, string iccid)
    {
        var client = new RestClient($"https://api.1nce.com/management-api/v1/sims/{iccid}");
        var request = new RestRequest
        {
            Method = Method.Get
        };
        request.AddHeader("accept", "application/json");
        request.AddHeader("authorization", $"Bearer {accessToken}");
        RestResponse response = client.Execute(request);

        if (response.Content is null) return null;
        SimCard? simCard = JsonSerializer.Deserialize<SimCard>(response.Content);
        return simCard?.status;
    }

    private static void Main(string[] args)
    {
        /*
         * https://help.1nce.com/dev-hub/openapi/6454cf1b397690015ee8d006
         * https://help.1nce.com/dev-hub/openapi/6454cf19c6f1ae01c7d38c00
         * https://editor.swagger.io/
         * https://help.1nce.com/dev-hub/reference/postaccesstokenpost
         */

        string? accessToken = GetAcessToken();
        if (accessToken is null) return;

        string iccid = "8988228066605439315";
        string ret = ActivateIccID(accessToken, iccid);
        Console.WriteLine(ret);

        string? status = "";
        while (!string.Equals(status, "Enabled"))
        {
            status = GetIccIdStatus(accessToken, iccid);
            Console.WriteLine(status);
            Thread.Sleep(2500);
        }

        Console.WriteLine(status);

        // reboot modem

        // wait until ping using GSM network interface is online and working
    }
}