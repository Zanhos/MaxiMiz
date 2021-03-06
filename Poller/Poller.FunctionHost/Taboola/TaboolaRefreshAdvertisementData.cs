using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Poller.Taboola;
using Poller.Poller;
using System.Threading;
using System.Threading.Tasks;

namespace Poller.FunctionHost.Taboola
{

    /// <summary>
    /// Azure function for the <see cref="IPollerRefreshAdvertisementData"/> 
    /// interface for the Taboola poller.
    /// </summary>
    public class TaboolaRefreshAdvertisementData
    {

        /// <summary>
        /// The injected poller object.
        /// </summary>
        private readonly TaboolaPoller _poller;

        /// <summary>
        /// The poller context singleton.
        /// TODO Clean up singleton
        /// </summary>
        private static PollerContext _pollerContext;
        private static PollerContext pollerContext
        {
            get
            {
                if (_pollerContext == null)
                {
                    _pollerContext = new PollerContext(0, null);
                }
                return _pollerContext;
            }
        }

        /// <summary>
        /// Constructor for DI.
        /// </summary>
        /// <param name="poller">Injected taboola poller object</param>
        public TaboolaRefreshAdvertisementData(TaboolaPoller poller)
        {
            _poller = poller;
        }

        /// <summary>
        /// Timed azure function for refreshing the advertisement data from Taboola.
        /// </summary>
        /// <param name="myTimer">The provided timer object</param>
        /// <param name="log">The logger</param>
        [FunctionName("TaboolaRefreshAdvertisementData")]
        public async Task Run([TimerTrigger("*/25 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            // Execute with cancellation token and clean up
            // TODO Token is useless here, do this differently --> use in function parameters (yes this is possible)
            using (var source = new CancellationTokenSource())
            {
                await _poller.RefreshAdvertisementDataAsync(pollerContext, source.Token);
            }
        }
    }
}
