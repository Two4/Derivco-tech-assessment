namespace DerivcoRoulette.Models.Db;

public partial class Payout
{
    public Result ToResult()
    {
        if (!Bets.Any()) throw new InvalidOperationException("No bets to pay out");
        return new Result(PayoutId, Bets);
    }
    
    public class Result
    {
        public long PayoutId { get; }
        public int Total { get; }
        public List<Bet> Bets { get; }

        internal Result(long payoutId, IEnumerable<Bet> bets)
        {
            PayoutId = payoutId;
            Total = (int) bets.Select(b => b.BetWinnings.HasValue ? b.BetWinnings.Value : 0).Sum();
            Bets = new List<Bet>(bets);
        }
    }
}