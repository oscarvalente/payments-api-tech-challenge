using System.Diagnostics.Tracing;

namespace PaymentsAPI.Services.Metrics
{

    [EventSource(Name = "PaymentsAPI.EventCounter.RequestProcessingTime")]
    public sealed class RequestProcessingTimeEventSource : EventSource
    {
        public static readonly RequestProcessingTimeEventSource Log = new RequestProcessingTimeEventSource();
        private int eventId;

        private EventCounter requestCounter;

        private RequestProcessingTimeEventSource() =>
            requestCounter = new EventCounter("latency", this)
            {
                DisplayName = "Request Processing Time",
                DisplayUnits = "ms"
            };

        public void Request(string url, long elapsedMilliseconds)
        {
            WriteEvent(1, url, elapsedMilliseconds);
            requestCounter?.WriteMetric(elapsedMilliseconds);
        }

        protected override void Dispose(bool disposing)
        {
            requestCounter?.Dispose();
            requestCounter = null;

            base.Dispose(disposing);
        }
    }
}