using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DerivcoRoulette.Models.Db
{
    public partial class Spin
    {
        public Spin()
        {
            Bets = new HashSet<Bet>();
        }

        public long SpinId { get; set; }
        public long? SpinResult { get; set; }
        public long TimestampUtc { get; set; }

        [JsonIgnore]
        public virtual ICollection<Bet> Bets { get; set; }
    }
}
