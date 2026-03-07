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

    public override UInt128 GenerateMask(AlgebraicPoint point)
    {
        if (_isWhite && point.Y != 5 && point.Y != 6)
        {
            return 0;
        }
        else if (!_isWhite && point.Y != 3 && point.Y != 4)
        {
            return 0;
        }

        return SlideMaskToEnd(_slideOffset, point) | SlideMaskToEnd(_slideOffset, point);
    }

    public override UInt128 ComputeAttacks(AlgebraicPoint point, UInt128 blocker)
    {
        UInt128 attacks = 0;

        point += _slideOffset;
        while (
            point.X < Constants.BoardSize
            && point.Y < Constants.BoardSize
            && point.X >= 0
            && point.Y >= 0
        )
        {
            int stepSquareIdx = point.Y * Constants.BoardSize + point.X;
            if ((blocker & (UInt128.One << stepSquareIdx)) != 0)
            {
                break;
            }

            int captureSquareIdx = (point.Y + _yOffset) * Constants.BoardSize + point.X;
            if ((blocker & (UInt128.One << captureSquareIdx)) == 0)
            {
                break;
            }

            attacks |= UInt128.One << captureSquareIdx;

            point += _slideOffset;
        }

        return attacks;
    }
}
