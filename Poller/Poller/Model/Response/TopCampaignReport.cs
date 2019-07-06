using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MaxiMiz.Poller.Model.Response
{
    [DataContract]
    public class TopCampaignReport
    {
        /// <summary>
        /// Epoch timestamp in milliseconds for when this data was last updated
        /// </summary>
        [DataMember(Name = "last-used-rawdata-update-time-gmt-millisec")]
        public ulong LastUpdateTimeEpochGmtInMillis { get; set; }

        /// <summary>
        /// The items in this result.
        /// </summary>
        [DataMember(Name = "results")]
        public IEnumerable<PublisherItem> Items { get; set; }
    }
}
