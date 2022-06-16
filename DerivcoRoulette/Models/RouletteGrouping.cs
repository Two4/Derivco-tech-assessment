using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace DerivcoRoulette.Models;

public class RouletteGrouping
{
    private readonly int[] _values;
    public ImmutableArray<int> Values => _values.ToImmutableArray();

    internal RouletteGrouping(int[] values)
    {
        _values = values;
    }

    public RouletteGrouping()
    {
        _values = Array.Empty<int>();
    }
}

public static class RouletteGroupings
{
    public static readonly RouletteGrouping HIGH;
    public static readonly RouletteGrouping LOW;
    public static readonly RouletteGrouping RED;
    public static readonly RouletteGrouping BLACK;
    public static readonly RouletteGrouping EVEN;
    public static readonly RouletteGrouping ODD;
    public static readonly RouletteGrouping FIRST_ROW;
    public static readonly RouletteGrouping SECOND_ROW;
    public static readonly RouletteGrouping THIRD_ROW;
    public static readonly RouletteGrouping FIRST_COL;
    public static readonly RouletteGrouping SECOND_COL;
    public static readonly RouletteGrouping THIRD_COL;

    private static readonly Dictionary<string, RouletteGrouping> _groupings;

    static RouletteGroupings()
    {
        _groupings = new Dictionary<string, RouletteGrouping>();
        
        HIGH = new RouletteGrouping(Enumerable.Range(19, 18).ToArray());
        _groupings.Add(nameof(HIGH), HIGH);
        LOW = new RouletteGrouping(Enumerable.Range(1, 18).ToArray());
        _groupings.Add(nameof(LOW), LOW);
        RED = new RouletteGrouping(Enumerable.Range(1, 36).Where(i => i % 2 == 0).ToArray());
        _groupings.Add(nameof(RED), RED);
        BLACK = new RouletteGrouping(Enumerable.Range(1, 36).Where(i => i % 2 != 0).ToArray());
        _groupings.Add(nameof(BLACK), BLACK);
        EVEN = RED;
        _groupings.Add(nameof(EVEN), EVEN);
        ODD = BLACK;
        _groupings.Add(nameof(ODD), ODD);
        FIRST_ROW = new RouletteGrouping(Enumerable.Range(1, 12).ToArray());
        _groupings.Add(nameof(FIRST_ROW), FIRST_ROW);
        SECOND_ROW = new RouletteGrouping(Enumerable.Range(13, 12).ToArray());
        _groupings.Add(nameof(SECOND_ROW), SECOND_ROW);
        THIRD_ROW = new RouletteGrouping(Enumerable.Range(25, 12).ToArray());
        _groupings.Add(nameof(THIRD_ROW), THIRD_ROW);
        FIRST_COL = new RouletteGrouping(GenerateColumn(1));
        _groupings.Add(nameof(FIRST_COL), FIRST_COL);
        SECOND_COL = new RouletteGrouping(GenerateColumn(2));
        _groupings.Add(nameof(SECOND_COL), SECOND_COL);
        THIRD_COL = new RouletteGrouping(GenerateColumn(3));
        _groupings.Add(nameof(THIRD_COL), THIRD_COL);
    }
    
    private static int[] GenerateColumn(int start)
    {
        if (start is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(start) ,$"arg {nameof(start)} must be between 1 and 3 (inclusive)");
        int[] output = new int[12];
        for (int i = start, j = 0; i <= 36 && j < output.Length; i += 3, j++)
        {
            output[j] = i;
        }

        return output;
    }
    
    public static RouletteGrouping Parse(string groupingString) {
        
        if (_groupings.TryGetValue(groupingString.ToUpperInvariant(), out RouletteGrouping? grouping))
        {
            return grouping;
        }

        throw new ArgumentException($"'{groupingString}' does not correspond to any known RouletteGrouping value");
    }
}