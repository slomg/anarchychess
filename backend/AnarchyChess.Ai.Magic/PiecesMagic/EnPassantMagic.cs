using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.PiecesMagic;

public sealed class EnPassantMagic : PieceMagic
{
    private string _name = "EnPassant";
    public override string Name => _name;

    private int _yOffset;
    private Offset _slideOffset;
    private bool _isWhite;

    public EnPassantMagic WhiteLeft()
    {
        _yOffset = -1;
        _slideOffset = new(X: -1, Y: 1);
        _name = "WhiteLeftEnPassant";
        _isWhite = true;
        return this;
    }

    public EnPassantMagic WhiteRight()
    {
        _yOffset = -1;
        _slideOffset = new(X: 1, Y: 1);
        _name = "WhiteRightEnPassant";
        _isWhite = true;
        return this;
    }

    public EnPassantMagic BlackLeft()
    {
        _yOffset = 1;
        _slideOffset = new(X: -1, Y: -1);
        _name = "BlackLeftEnPassant";
        return this;
    }

    public EnPassantMagic BlackRight()
    {
        _yOffset = 1;
        _slideOffset = new(X: 1, Y: -1);
        _name = "BlackRightEnPassant";
        return this;
    }

    public override UInt128 GenerateMask(int x, int y)
    {
        if (_isWhite && y != 5 && y != 6)
        {
            return 0;
        }
        else if (!_isWhite && y != 3 && y != 4)
        {
            return 0;
        }

        return SlideMaskToEnd(x, y, _slideOffset) | SlideMaskToEnd(x, y + _yOffset, _slideOffset);
    }

    public override UInt128 ComputeAttacks(int x, int y, UInt128 blocker)
    {
        UInt128 attacks = 0;

        x += _slideOffset.X;
        y += _slideOffset.Y;
        while (x < Constants.BoardSize && y < Constants.BoardSize && x >= 0 && y >= 0)
        {
            int stepSquareIdx = y * Constants.BoardSize + x;
            if ((blocker & (UInt128.One << stepSquareIdx)) != 0)
            {
                break;
            }

            int captureSquareIdx = (y + _yOffset) * Constants.BoardSize + x;
            if ((blocker & (UInt128.One << captureSquareIdx)) == 0)
            {
                break;
            }

            attacks |= UInt128.One << captureSquareIdx;

            y += _slideOffset.Y;
            x += _slideOffset.X;
        }

        return attacks;
    }
}
