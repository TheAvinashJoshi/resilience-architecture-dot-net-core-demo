using System.Net;

/* A simulated API service that fails 3 times before succeeding */


namespace resilience_architecture_dot_net_core_demo
{
    public class TestAPIService
    {
        private int _attemptCount = 0;
        private const int FailUntilAttempt = 3;

        public async Task<string> GetDataAsync()
        {
            _attemptCount++;
            Console.WriteLine($"\n--- Service Call Attempt {_attemptCount} ---");

            if (_attemptCount <= FailUntilAttempt)
            {
                Console.WriteLine($"[Service] Fails with HTTP 500.");
                throw new HttpRequestException("Simulated 500 Internal Server Error", null, HttpStatusCode.InternalServerError);
            }
            else
            {
                Console.WriteLine($"[Service] Succeeds!");
                return await Task.FromResult("Success: Data Retrieved on attempt " + _attemptCount);
            }
        }
    }
}
