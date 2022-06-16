using System.Collections.Immutable;
using DerivcoRoulette.Models.Db;

namespace DerivcoRoulette.Models.Db;

public partial class Bet
{
    public ImmutableArray<int> BetOnValues => GetBetOnValues(BetOn);

    private static ImmutableArray<int> GetBetOnValues(string betOn)
    {
        if (int.TryParse(betOn, out int number))
        {
            if (number is < 0 or > 36)
            {
                throw new InvalidOperationException($"Provided number {number} is not between 0 and 36 (inclusive)");
            }

            return new[] {number}.ToImmutableArray();
        }

        return RouletteGroupings.Parse(betOn).Values;
    }

    public static async Task<Bet> Create(string betOn, int betValue, RouletteContext dbContext)
    {
        //input sanity check
        if (betValue < 1) throw new ArgumentOutOfRangeException(nameof(betValue), $"{nameof(betValue)} must exceed zero");
        GetBetOnValues(betOn);

        Bet bet;
        dbContext.Bets.Add(bet = new Bet
        {
            SpinId = dbContext.GetNextSpin().Result.SpinId,
            BetOn = betOn,
            TimestampUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            BetValue = betValue
        });
        await dbContext.SaveChangesAsync();
        return bet;
    }

    public void CalculateWinnings(int spinResult)
    {
        if (BetOnValues.Contains(spinResult))
        {
            decimal adjustedBetValue = BetValue / (decimal) BetOnValues.Length;
            BetWinnings = (int) Math.Round(adjustedBetValue * 36);
        }
        else
        {
            BetWinnings = 0;
        }
    }
}