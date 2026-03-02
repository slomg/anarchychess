using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.PiecesMagic;

public abstract class PieceMagic : IPieceMagic
{
    public abstract string Name { get; }

    public abstract UInt128 GenerateMask(int x, int y);
    public abstract UInt128 ComputeAttacks(int x, int y, UInt128 blocker);

    protected static UInt128 SlideMask(int x, int y, Offset offset, int limit = int.MaxValue)
    {
        UInt128 mask = 0;

        x += offset.X;
        y += offset.Y;
        int i = 0;
        while (x < Constants.BoardSize && y < Constants.BoardSize && x >= 0 && y >= 0 && i < limit)
        {
            // stop before the edge square in the direction we're sliding
            if (
                (offset.X != 0 && (x == 0 || x == Constants.BoardSize - 1))
                || (offset.Y != 0 && (y == 0 || y == Constants.BoardSize - 1))
            )
            {
                break;
            }

            mask |= UInt128.One << (y * Constants.BoardSize + x);
            x += offset.X;
            y += offset.Y;

            i++;
        }

        return mask;
    }

    protected static UInt128 SlideMaskToEnd(int x, int y, Offset offset)
    {
        UInt128 mask = 0;

        x += offset.X;
        y += offset.Y;
        while (x < Constants.BoardSize && y < Constants.BoardSize && x >= 0 && y >= 0)
        {
            mask |= UInt128.One << (y * Constants.BoardSize + x);
            x += offset.X;
            y += offset.Y;
        }

        return mask;
    }

    protected static UInt128 SlideAttack(
        int x,
        int y,
        Offset offset,
        UInt128 blocker,
        int limit = int.MaxValue
    )
    {
        UInt128 attacks = 0;

        x += offset.X;
        y += offset.Y;
        int i = 0;
        while (x < Constants.BoardSize && y < Constants.BoardSize && x >= 0 && y >= 0 && i < limit)
        {
            int squareIdx = y * Constants.BoardSize + x;
            attacks |= UInt128.One << squareIdx;
            if ((blocker & (UInt128.One << squareIdx)) != 0)
            {
                break;
            }

            x += offset.X;
            y += offset.Y;

            i++;
        }

        return attacks;
    }
}
