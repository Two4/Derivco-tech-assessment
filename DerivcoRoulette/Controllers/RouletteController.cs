using System.Security.Cryptography;
using System.Text.Json;
using DerivcoRoulette.Models.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DerivcoRoulette.Controllers;

[ApiController]
[Route("roulette")]
public class RouletteController : ControllerBase
{
    private static readonly RandomGenerator Random = new RandomGenerator();
    private readonly RouletteContext _context;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="context"></param>
    public RouletteController(RouletteContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Places a bet for the next roulette wheel spin.
    /// </summary>
    /// <param name="betOn">
    /// can either be an integer from the range <c>[0..36]</c> (inclusive) or any one of
    /// <code>
    /// {HIGH;
    /// LOW;
    /// RED;
    /// BLACK;
    /// EVEN;
    /// ODD;
    /// FIRST_ROW;
    /// SECOND_ROW;
    /// THIRD_ROW;
    /// FIRST_COL;
    /// SECOND_COL;
    /// THIRD_COL;}
    /// </code> as a case-insensitive string
    /// </param>
    /// <param name="betValue">the money value of the bet</param>
    /// <returns>a JSON-formatted <c>Bet</c> object representing the placed bet</returns>
    [HttpGet]
    [Route("bet/place/{betOn}/{betValue:int}")]
    public async Task<IActionResult> PlaceBet(string betOn, int betValue)
    {
        return new JsonResult(await Bet.Create(betOn, betValue, _context));
    }

    /// <summary>
    /// Spins the wheel and assigns winnings to all bets placed for this spin
    /// </summary>
    /// <returns>a JSON-formatted <c>Spin.Result</c> object representing the spin result</returns>
    [HttpGet]
    [Route("spin")]
    public async Task<IActionResult> Spin()
    {
        Spin nextSpin;
        _context.Update(nextSpin = await _context.GetNextSpin());
        if (nextSpin.Bets.Count < 1) throw new InvalidOperationException("Cannot spin with no placed bets");
        nextSpin.SpinResult = Random.Next(0, 37);
        
        List<Bet> bets = await _context.Bets.Where(b => b.SpinId == nextSpin.SpinId).ToListAsync();
        foreach (Bet bet in bets)
        {
            _context.Update(bet);
            bet.CalculateWinnings((int) nextSpin.SpinResult);
        }
        await _context.SaveChangesAsync();
        return new JsonResult(nextSpin.ToResult());
    }

    /// <summary>
    /// Pays out all outstanding won bets.
    /// </summary>
    /// <returns>a JSON-formatted <c>Payout.Result</c> object representing the payout result</returns>
    [HttpGet]
    [Route("payout")]
    public async Task<IActionResult> Payout()
    {
        List<Bet> unpaid = _context.Bets
            .Where(b => b.Spin.SpinResult.HasValue && b.BetWinnings > 0 && b.Payout == null)
            .ToList();
        if (unpaid.Count < 1) throw new InvalidOperationException("No bets to pay out");
            Payout payout;
        _context.Payouts.Add(payout = new Payout()
        {
            Bets = unpaid,
            TimestampUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
        await _context.SaveChangesAsync();
        return new JsonResult(payout.ToResult());
    }

    /// <summary>
    /// Gets a paginated list of spins sorted by timestamp, descending.
    /// </summary>
    /// <param name="page">the page in the history to display; a requesting a page that does not exist results in an Exception</param>
    /// <returns>a JSON-formatted list of <c>Spin</c>objects, up to 100 items long</returns>
    [HttpGet]
    [Route("spin/history")]
    public async Task<IActionResult> ShowPreviousSpins([FromQuery] int page = 1)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), $"{nameof(page)} must exceed zero");
        const int pageSize = 100;
        List<Spin> historyPage = await _context.Spins
            .OrderByDescending(s => s.TimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        if (historyPage.Count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page),
                $"{nameof(page)} {page} of previous spins does not exist");
        }

        return new JsonResult(historyPage);
    }
    
    /// <summary>
    /// Secure random generator
    /// 
    /// https://stackoverflow.com/questions/42426420/how-to-generate-a-cryptographically-secure-random-integer-within-a-range
    /// </summary>
    private sealed class RandomGenerator : IDisposable
    {
        private readonly RandomNumberGenerator _rng;
        /// <summary>
        /// Constructor
        /// </summary>
        public RandomGenerator()
        {
            _rng = RandomNumberGenerator.Create();
        }
        /// <summary>
        /// Get random value
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxExclusiveValue"></param>
        /// <returns></returns>
        public int Next(int minValue, int maxExclusiveValue)
        {
            if (minValue == maxExclusiveValue) return minValue;

            if (minValue > maxExclusiveValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue) ,$"{nameof(minValue)} must be lower than {nameof(maxExclusiveValue)}");
            }

            long diff = (long)maxExclusiveValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;

            uint ui;
            do
            {
                ui = GetRandomUInt();
            } while (ui >= upperBound);
            return (int)(minValue + (ui % diff));
        }

        private uint GetRandomUInt()
        {
            byte[] randomBytes = GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randomBytes, 0);
        }

        private byte[] GenerateRandomBytes(int bytesNumber)
        {
            byte[] buffer = new byte[bytesNumber];
            _rng.GetBytes(buffer);
            return buffer;
        }
        private bool _disposed;
        
        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed state (managed objects).
                _rng?.Dispose();
            }

            _disposed = true;
        }
    }
}