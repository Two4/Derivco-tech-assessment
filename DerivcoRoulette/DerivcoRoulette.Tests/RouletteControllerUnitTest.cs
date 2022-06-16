using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DerivcoRoulette.Controllers;
using DerivcoRoulette.Models.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DerivcoRoulette.Tests;

public class RouletteControllerUnitTest
{
    private static readonly Random Random = new Random();

    private static RouletteContext CreateTestDb()
    {
        DbContextOptionsBuilder<RouletteContext> optionsBuilder = new DbContextOptionsBuilder<RouletteContext>();
        string dbPath = Path.Combine(AppContext.BaseDirectory, "test.roulette.sqlite");
        if (!File.Exists(dbPath)) throw new FileNotFoundException($"Could not find {dbPath}");
        string testDbPath = Path.Combine(AppContext.BaseDirectory, $"{RandomString(8)}.sqlite");
        File.Copy(dbPath, testDbPath);
        optionsBuilder.UseSqlite($"DataSource={testDbPath};Cache=Shared");
        return new RouletteContext(optionsBuilder.Options);
    }
    
    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }

    private static List<string> GetValidBetOnParams()
    {
        List<string> validParams = new List<string>
        {
            "HIGH",
            "LOW",
            //
            "RED",
            "BLACK",
            //
            "EVEN",
            "ODD",
            //
            "FIRST_ROW",
            "SECOND_ROW",
            "THIRD_ROW",
            //
            "FIRST_COL",
            "SECOND_COL",
            "THIRD_COL"
        };
        validParams.AddRange(Enumerable.Range(0, 36).Select(i => i.ToString()));

        return validParams;
    }

    [Fact]
    public void NominalTest()
    {
        // Arrange
        RouletteContext context = CreateTestDb();
        RouletteController controller = new RouletteController(context);
        List<string> betOnParams = GetValidBetOnParams();
        const int betValue = 10;
        
        // Act & Assert : PlaceBet
        Spin nextSpin = context.GetNextSpin().Result;
        foreach (string param in betOnParams)
        {
            // Assert that bets are as expected after being placed and before wheel is spun
            IActionResult placeActionResult = controller.PlaceBet(param, betValue).Result;
            JsonResult placeJsonResult = Assert.IsType<JsonResult>(placeActionResult as JsonResult);
            Bet bet = Assert.IsType<Bet>(placeJsonResult.Value as Bet);
            Assert.True(bet.BetOn == param);
            Assert.True(bet.BetValue == betValue);
            Assert.True(bet.SpinId == nextSpin.SpinId);
            Assert.True(bet.BetWinnings == null);
            Assert.True(bet.PayoutId == null);
        }
        
        // Act : Spin
        IActionResult spinActionResult = controller.Spin().Result;

        // Assert : Spin
        JsonResult spinJsonResult = Assert.IsType<JsonResult>(spinActionResult as JsonResult);
        Spin.Result spinResult = Assert.IsType<Spin.Result>(spinJsonResult.Value as Spin.Result);
        
            // Assert that all bets are present
        Assert.True(spinResult.Bets.Count == betOnParams.Count);
            
            // Assert that the bets are assigned to the correct spin
        Assert.True(spinResult.Bets.TrueForAll(b => b.SpinId == nextSpin.SpinId));
        
            // Assert that all bets have assigned winnings
        Assert.True(spinResult.Bets.TrueForAll(b => b.BetWinnings.HasValue));
        
        
        int expectedWinnings;
        List<Bet> wonBets = spinResult.Bets.Where(b => b.BetWinnings is > 0).ToList();
        if (spinResult.WheelValue == 0) // Wheel landing on zero means only one winning bet
        {
            expectedWinnings = 36 * betValue;
            Assert.Single(wonBets);
            Assert.True(wonBets.Select(b => b.BetWinnings).Sum() == expectedWinnings);
        }
        else
        {
            //5 grouping wins + 1 number win
            Assert.True(wonBets.Count == 6);
            //3 x 2:1 wins + 2 x 3:1 wins + 1 x 36:1 win
            expectedWinnings = (3 * (betValue * 2)) + (2 * (betValue * 3)) + (betValue * 36);
            Assert.True(wonBets.Select(b => b.BetWinnings).Sum() == expectedWinnings);
        }
        
        // Act: Payout
        IActionResult payoutActionResult = controller.Payout().Result;
        
        // Assert: Payout
        JsonResult payoutJsonResult = Assert.IsType<JsonResult>(payoutActionResult as JsonResult);
        Payout.Result payoutResult = Assert.IsType<Payout.Result>(payoutJsonResult.Value as Payout.Result);
        
            // Payout equals expected winnings
        Assert.True(payoutResult.Total == expectedWinnings);
            // Sum of bet winnings is equal to total
        Assert.True(payoutResult.Total == payoutResult.Bets.Select(b => b.BetWinnings).Sum());
            // All won bets are present
        Assert.True(payoutResult.Bets
            .OrderBy(b => b.TimestampUtc)
            .Select(b => b.BetId)
            .SequenceEqual(
                wonBets
                    .OrderBy(b => b.TimestampUtc)
                    .Select(b => b.BetId)
                )
        );
        
        // Act: ShowPreviousSpins
        IActionResult historyActionResult = controller.ShowPreviousSpins().Result;
        
        // Assert: ShowPreviousSpins
        JsonResult historyJsonResult = Assert.IsType<JsonResult>(historyActionResult as JsonResult);
        List<Spin> historyResult = Assert.IsType<List<Spin>>(historyJsonResult.Value as List<Spin>);
        Assert.Single(historyResult);
        Assert.True(historyResult.First().SpinId == spinResult.SpinId);
        Assert.True(historyResult.First().SpinResult == spinResult.WheelValue);
    }

    [Fact]
    public void PlaceBetOutOfRange()
    {
        // Arrange
        RouletteContext context = CreateTestDb();
        RouletteController controller = new RouletteController(context);
        List<string> validParams = GetValidBetOnParams();
        const int betValue = 10;

        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            string param;
            do
            {
                param = RandomString(Random.Next(1, 20));
            } while (validParams.Contains(param));
            
            Assert.ThrowsAny<Exception>(() => { _ = controller.PlaceBet(param, betValue).Result; });
            Assert.ThrowsAny<Exception>(() =>
            {
                _ = controller.PlaceBet(Random.Next(int.MinValue, 0).ToString(), betValue).Result;
            });
            Assert.ThrowsAny<Exception>(() =>
            {
                _ = controller.PlaceBet(Random.Next(37, int.MaxValue).ToString(), betValue).Result;
            });
        }
    }

    [Fact]
    public void PlaceBetValueOutOfRange()
    {
        // Arrange
        RouletteContext context = CreateTestDb();
        RouletteController controller = new RouletteController(context);
        const string betOn = "HIGH";
        
        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            Assert.ThrowsAny<Exception>(() =>
            {
                _ = controller.PlaceBet(betOn, Random.Next(int.MinValue, 1)).Result;
            });
        }
        
        Assert.ThrowsAny<Exception>(() =>
        {
            _ = controller.PlaceBet(betOn, 0).Result;
        });
    }

    [Fact]
    public void NoBetSpin()
    {
        // Arrange
        RouletteContext context = CreateTestDb();
        RouletteController controller = new RouletteController(context);

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => { _ = controller.Spin().Result; });
    }

    [Fact]
    public void NoBetPayout()
    {
        // Arrange
        RouletteContext context = CreateTestDb();
        RouletteController controller = new RouletteController(context);

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => { _ = controller.Payout().Result; });

    }

    [Fact]
    public void NoSpinHistory()
    {
        // Arrange
        RouletteContext context = CreateTestDb();
        RouletteController controller = new RouletteController(context);

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => { _ = controller.ShowPreviousSpins().Result; });
    }
}