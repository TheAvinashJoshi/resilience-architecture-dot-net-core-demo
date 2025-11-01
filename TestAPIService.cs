using System.Net;

/* A simulated API service that fails 3 times before succeeding 
 
 SIMULATION SERVICE EXPLANATION
 This service is specifically designed to simulate a transient fault scenario.
 
 The GetDataAsync method will intentionally throw an HttpRequestException 
 (simulating a 500 Internal Server Error) for the first 'FailUntilAttempt' (3) 
 times it is called.
 
 On the fourth attempt (Attempt 4), it will succeed, allowing the Retry Policy 
 in the resilience pipeline to demonstrate its effectiveness in overcoming 
 temporary issues without exposing the error to the calling application.
*/

namespace resilience_architecture_dot_net_core_demo
{
    public class TestAPIService
    {
        // this is to keep track of the number of attempts
        private int _attemptCount = 0;
        // this constant defines how many times the service will fail before succeeding
        private const int FailUntilAttempt = 3;

        public async Task<string> GetDataAsync()
        {
            // increment the attempt count
            _attemptCount++;
            // log the attempt
            Console.WriteLine($"\n--- Service Call Attempt {_attemptCount} ---");

            // simulate failure for the first 'FailUntilAttempt' attempts
            if (_attemptCount <= FailUntilAttempt)
            {
                // if the attempt count is less than or equal to FailUntilAttempt, throw an exception
                Console.WriteLine($"[Service] Fails with HTTP 500.");
                throw new HttpRequestException("Simulated 500 Internal Server Error", null, HttpStatusCode.InternalServerError);
            }
            else
            {
                // if the attempt count is greater than FailUntilAttempt, return success
                Console.WriteLine($"[Service] Succeeds!");
                return await Task.FromResult("Success: Data Retrieved on attempt " + _attemptCount);
            }
        }
    }
}
