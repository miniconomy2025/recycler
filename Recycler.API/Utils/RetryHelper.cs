namespace Recycler.API.Utils
{
    public static class RetryHelper
    {
        public static async Task<T> RetryAsync<T>(
            Func<Task<T>> action,
            int maxAttempts = 5,
            int initialDelayMs = 1000,
            string? operationName = null)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    Console.WriteLine($"Attempting {operationName ?? "operation"} (Attempt {attempt}/{maxAttempts})...");
                    var result = await action();

                    Console.WriteLine($"{operationName ?? "Operation"} succeeded on attempt {attempt}.");
                    Console.WriteLine($"Response: {result}");
                    return result;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    Console.WriteLine($"Retry {attempt}/{maxAttempts} failed for {operationName ?? "operation"}: {ex.Message}");
                    await Task.Delay(initialDelayMs * Math.Max(attempt, 10));
                }
            }

            throw new Exception($"All {maxAttempts} retry attempts failed for {operationName ?? "operation"}.");
        }
    }
}
