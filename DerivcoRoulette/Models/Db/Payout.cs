using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DerivcoRoulette.Models.Db
{
    public partial class Payout
    {
        public Payout()
        {
            Bets = new HashSet<Bet>();
        }

        public long PayoutId { get; set; }
        public long TimestampUtc { get; set; }

        [JsonIgnore]
        public virtual ICollection<Bet> Bets { get; set; }
    }
}
