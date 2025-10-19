public class GlobalHeaderHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("Client-Id", "recycler");
        return await base.SendAsync(request, cancellationToken);
    }
}