namespace DerivcoRoulette.Models.Db;

public partial class RouletteContext
{
    public async Task<Spin> GetNextSpin()
    {
        Spin? nextSpin = Spins
            .Where(s => !s.SpinResult.HasValue)
            .OrderByDescending(s => s.SpinId)
            .FirstOrDefault();
        if (nextSpin == null)
        {
            Spins.Add(nextSpin = new Spin()
            {
                TimestampUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
            await SaveChangesAsync();
        }

        return nextSpin;
    }
}