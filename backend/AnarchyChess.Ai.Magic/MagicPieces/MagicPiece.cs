using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.MagicPieces;

public abstract class MagicPiece : IMagicPiece
{
    public abstract string Name { get; }

    public abstract UInt128 GenerateMask(int x, int y);
    public abstract UInt128 ComputeAttacks(int x, int y, UInt128 blocker);

    protected static UInt128 SlideMask(int x, int y, Offset offset)
    {
        UInt128 mask = 0;

        x += offset.X;
        y += offset.Y;
        while (x < Constants.BoardSize - 1 && y < Constants.BoardSize - 1 && x > 0 && y > 0)
        {
            mask |= UInt128.One << (y * Constants.BoardSize + x);
            x += offset.X;
            y += offset.Y;
        }

        return mask;
    }

    protected static UInt128 SlideAttack(int x, int y, Offset offset, UInt128 blocker)
    {
        UInt128 attacks = 0;

        x += offset.X;
        y += offset.Y;
        while (x < Constants.BoardSize && y < Constants.BoardSize && x >= 0 && y >= 0)
        {
            int squareIdx = y * Constants.BoardSize + x;
            attacks |= UInt128.One << squareIdx;
            if ((blocker & (UInt128.One << squareIdx)) != 0)
            {
                break;
            }

            x += offset.X;
            y += offset.Y;
        }

        return attacks;
    }
}
