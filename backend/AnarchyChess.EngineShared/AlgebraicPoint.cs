using System.Text.Json.Serialization;
using Orleans;

namespace AnarchyChess.EngineShared;

[GenerateSerializer]
[Alias("AnarchyChess.EngineShared.AlgebraicPoint")]
[method: JsonConstructor]
public readonly record struct AlgebraicPoint(int X, int Y)
{
    public AlgebraicPoint(string algebraic)
        : this(algebraic[0] - 'a', int.Parse(algebraic[1..]) - 1) { }

    public static bool TryParse(
        string algebraic,
        int maxWidth,
        int maxHeight,
        out AlgebraicPoint point
    )
    {
        point = default;
        AlgebraicPoint parsed;
        try
        {
            parsed = new(algebraic);
        }
        catch (FormatException)
        {
            return false;
        }

        if (parsed.Y >= 0 && parsed.Y < maxHeight && parsed.X >= 0 && parsed.X < maxWidth)
        {
            point = parsed;
            return true;
        }
        return false;
    }

    public static AlgebraicPoint operator +(AlgebraicPoint left, Offset right) =>
        new(left.X + right.X, left.Y + right.Y);

    public static AlgebraicPoint operator -(AlgebraicPoint left, Offset right) =>
        new(left.X - right.X, left.Y - right.Y);

    public string AsAlgebraic()
    {
        var rank = (char)('a' + X);
        return $"{rank}{Y + 1}";
    }

    public byte AsIdx(int boardWidth) => (byte)(Y * boardWidth + X);

    public override string ToString() => AsAlgebraic();
}
