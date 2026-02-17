using AnarchyChess.Ai.Magic;
using AnarchyChess.Ai.Magic.PiecesMagic;

MagicGenerator.Generate(new EnPassantMagic().WhiteRight());
MagicGenerator.Generate(new EnPassantMagic().WhiteLeft());

MagicGenerator.Generate(new EnPassantMagic().BlackRight());
MagicGenerator.Generate(new EnPassantMagic().BlackLeft());
