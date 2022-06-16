using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DerivcoRoulette.Models.Db
{
    public partial class Bet
    {
        public long BetId { get; set; }
        public long SpinId { get; set; }
        public string BetOn { get; set; } = null!;
        public long TimestampUtc { get; set; }
        public long BetValue { get; set; }
        public long? BetWinnings { get; set; }
        public long? PayoutId { get; set; }

        [JsonIgnore]
        public virtual Payout? Payout { get; set; }
        [JsonIgnore]
        public virtual Spin Spin { get; set; } = null!;
    }
}
