namespace DerivcoRoulette.Models.Db;

public partial class Spin
{
    public Result ToResult()
    {
        if (!SpinResult.HasValue) throw new InvalidOperationException($"Spin (ID:{SpinId}) does not yet have a result");
        return new Result(SpinId, (int) SpinResult.Value, Bets);
    }
    
    public class Result
    {
        public long SpinId { get; }
        public int WheelValue { get; }
        public List<Bet> Bets { get; }

        internal Result(long spinId, int wheelValue, IEnumerable<Bet> bets)
        {
            SpinId = spinId;
            WheelValue = wheelValue;
            Bets = new List<Bet>(bets);
        }
    }
}