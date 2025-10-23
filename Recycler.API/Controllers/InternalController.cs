using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

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

    [HttpGet]
    [Route("add-machine")]
    public async Task<IActionResult> AddAMachine()
    {
        var response = await _http.PostAsJsonAsync("https://api.recycler.susnet.co.za/logistics", new
        {
            type = "DELIVERY",
            items = new[] {new
            {
            name = "recycling_machine",
                quantity = 1
            } }
        });
        var json = await response.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(json);
        return Ok(parsed);
    }

    [HttpGet]
    [Route("get-loans")]
    public async Task<IActionResult> GetLoans()
    {
        var response = await _http.GetAsync("https://commercial-bank-api.subspace.site/api/loan");
        var json = await response.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(json);
        return Ok(parsed);
    }

    [HttpGet]
    [Route("check-frozen-account")]
    public async Task<IActionResult> CheckFrozen()
    {
        var response = await _http.GetAsync("https://commercial-bank-api.subspace.site/api/account/me/frozen");
        var json = await response.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(json);
        return Ok(parsed);
    }

    [HttpPost]
    [Route("take-out-a-loan")]
    public async Task<IActionResult> TakeOutALoan([FromBody] LoanRequest request)
    {
        var response = await _http.PostAsJsonAsync("https://commercial-bank-api.subspace.site/api/loan", new
        {
            amount = request.Amount
        });
        var json = await response.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(json);
        return Ok(parsed);
    }

[HttpGet]
[Route("payback-all-loans")]
public async Task<IActionResult> PayBackAllLoan()
{
    // STEP 1: Get all loans
    var response = await _http.GetAsync("https://commercial-bank-api.subspace.site/api/loan");
    var json = await response.Content.ReadAsStringAsync();
    var parsed = JsonDocument.Parse(json);

    var loans = parsed.RootElement.GetProperty("loans");

    var results = new List<object>();

    // STEP 2: Loop through each loan
    foreach (var loan in loans.EnumerateArray())
    {
        var loanNumber = loan.GetProperty("loan_number").GetString();
        var outstanding = loan.GetProperty("outstanding_amount").GetDecimal();

        if (outstanding > 0)
        {
            // STEP 3: Pay back each loan
            var payResponse = await _http.PostAsJsonAsync(
                $"https://commercial-bank-api.subspace.site/api/loan/{loanNumber}/pay",
                new { amount = outstanding }
            );

            var payJson = await payResponse.Content.ReadAsStringAsync();
            results.Add(new
            {
                loanNumber,
                paymentAttempt = JsonDocument.Parse(payJson)
            });
        }
        else
        {
            results.Add(new
            {
                loanNumber,
                paymentAttempt = "No outstanding balance"
            });
        }
    }

    // STEP 4: Return summary
    return Ok(new
    {
        message = "All loans processed",
        results
    });
}
}

public class LoanRequest
{
    public decimal Amount { get; set; }
}

public class LoanPaymentRequest
{
    public long loanNumber { get; set; }
    public decimal Amount { get; set; }
}