using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyAwesomeCLI.Handlers;

internal interface IExchangeCredentials
{
    Task<string> GetCredentialsAsync(string userkey, string secret);
}

public class CredentialsExchangeService : IExchangeCredentials
{
    // THIS IS NOT REALLY A VALID JWT CREATION FUNCTION, IT IS FOR DEMO PURPOSES ONLY!!!
    public Task<string> GetCredentialsAsync(string userkey, string secret)
    {
        var header = Serialize(new { typ = "at", alg = "fake" });
        var body = Serialize(new
        {
            iss = "https://your.awesome.idp.provider/",
            aud = "https://your.awesome.cli.backend.service/",
            sub = userkey,
            scope = "read write",
            iat = Epoch(DateTime.UtcNow),
            exp = Epoch(DateTime.UtcNow.AddDays(30))
        });
        var signature = SHA256.HashData(Encoding.UTF8.GetBytes($"{header}.{body}"));

        return Task.FromResult($"{ToB64(header)}.{ToB64(body)}.{Convert.ToBase64String(signature)}");
    }

    private static string ToB64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
    
    private static readonly DateTime _epoch = new(1970, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
    private static long Epoch(DateTime dt) => (long)(dt - _epoch).TotalMilliseconds;

    private static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, Opts);

    private static JsonSerializerOptions Opts => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
