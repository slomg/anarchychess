using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.PiecesMagic;

public abstract class PieceMagic : IPieceMagic
{
    public abstract string Name { get; }

    public abstract UInt128 GenerateMask(AlgebraicPoint point);
    public abstract UInt128 ComputeAttacks(AlgebraicPoint point, UInt128 blocker);

    protected static UInt128 SlideMask(
        AlgebraicPoint point,
        Offset offset,
        int limit = int.MaxValue
    )
    {
        UInt128 mask = 0;

        point += offset;
        int i = 0;
        while (
            point.X < Constants.BoardSize
            && point.Y < Constants.BoardSize
            && point.X >= 0
            && point.Y >= 0
            && i < limit
        )
        {
            // stop before the edge square in the direction we're sliding
            if (
                (offset.X != 0 && (point.X == 0 || point.X == Constants.BoardSize - 1))
                || (offset.Y != 0 && (point.Y == 0 || point.Y == Constants.BoardSize - 1))
            )
            {
                break;
            }

            mask |= UInt128.One << (point.Y * Constants.BoardSize + point.X);
            point += offset;

            i++;
        }

        return mask;
    }

    protected static UInt128 SlideMaskToEnd(Offset offset, AlgebraicPoint point)
    {
        UInt128 mask = 0;

        point += offset;
        while (
            point.X < Constants.BoardSize
            && point.Y < Constants.BoardSize
            && point.X >= 0
            && point.Y >= 0
        )
        {
            mask |= UInt128.One << (point.Y * Constants.BoardSize + point.X);
            point += offset;
        }

        return mask;
    }

    protected static UInt128 SlideAttack(
        AlgebraicPoint point,
        Offset offset,
        UInt128 blocker,
        int limit = int.MaxValue
    )
    {
        UInt128 attacks = 0;

        point += offset;
        int i = 0;
        while (
            point.X < Constants.BoardSize
            && point.Y < Constants.BoardSize
            && point.X >= 0
            && point.Y >= 0
            && i < limit
        )
        {
            int squareIdx = point.Y * Constants.BoardSize + point.X;
            attacks |= UInt128.One << squareIdx;
            if ((blocker & (UInt128.One << squareIdx)) != 0)
            {
                break;
            }

            point += offset;
            i++;
        }

        return attacks;
    }
}
