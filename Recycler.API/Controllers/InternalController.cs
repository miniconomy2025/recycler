using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Recycler.API;

[ApiController]
[Route("internal")]
public class InternalController(IHttpClientFactory factory) : ControllerBase
{
    private readonly HttpClient _http = factory.CreateClient("internal");
    [HttpGet]
    [Route("account")]
    public async Task<IActionResult> GetAccount()
    {
        var response = await _http.GetAsync("https://commercial-bank-api.subspace.site/api/account/me");
        var json = await response.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(json);
        return Ok(parsed);
    }

    [HttpGet]
    [Route("start-simulation")]
    public async Task<IActionResult> PostStartSimulation()
    {
        var response = await _http.PostAsJsonAsync("https://api.recycler.susnet.co.za/simulation", new
        {
            epochStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
        var json = await response.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(json);
        return Ok(parsed);
    }
}