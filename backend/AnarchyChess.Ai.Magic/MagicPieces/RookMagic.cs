namespace AnarchyChess.Ai.Magic.MagicPieces;

public sealed class RookMagic : IMagicPiece
{
    public string Name => "Rook";

    public UInt128 GenerateMask(int x, int y)
    {
        UInt128 mask = 0;

        for (int dx = x + 1; dx < Constants.BoardSize - 1; dx++)
        {
            mask |= UInt128.One << (y * Constants.BoardSize + dx);
        }
        for (int dx = x - 1; dx > 0; dx--)
        {
            mask |= UInt128.One << (y * Constants.BoardSize + dx);
        }

        for (int dy = y + 1; dy < Constants.BoardSize - 1; dy++)
        {
            mask |= UInt128.One << (dy * Constants.BoardSize + x);
        }
        for (int dy = y - 1; dy > 0; dy--)
        {
            mask |= UInt128.One << (dy * Constants.BoardSize + x);
        }

        return mask;
    }

    public UInt128 ComputeAttacks(int x, int y, UInt128 blocker)
    {
        UInt128 attacks = 0;

        // left
        for (int i = x - 1; i >= 0; i--)
        {
            int sq = y * Constants.BoardSize + i;
            attacks |= UInt128.One << sq;
            if ((blocker & (UInt128.One << sq)) != 0)
            {
                break;
            }
        }

        // right
        for (int i = x + 1; i < Constants.BoardSize; i++)
        {
            int sq = y * Constants.BoardSize + i;
            attacks |= UInt128.One << sq;
            if ((blocker & (UInt128.One << sq)) != 0)
            {
                break;
            }
        }

        // down
        for (int i = y - 1; i >= 0; i--)
        {
            int sq = i * Constants.BoardSize + x;
            attacks |= UInt128.One << sq;
            if ((blocker & (UInt128.One << sq)) != 0)
            {
                break;
            }
        }

        // up
        for (int i = y + 1; i < Constants.BoardSize; i++)
        {
            int sq = i * Constants.BoardSize + x;
            attacks |= UInt128.One << sq;
            if ((blocker & (UInt128.One << sq)) != 0)
            {
                break;
            }
        }

        return attacks;
    }
}
