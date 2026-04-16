using System.Diagnostics;
using AnarchyChess.Ai;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

AiEngine engine = new();

Dictionary<AlgebraicPoint, Piece> startingPosition = new()
{
    #region White Pieces
    [new AlgebraicPoint("a1")] = new Piece(PieceType.Rook, GameColor.White),
    [new AlgebraicPoint("b1")] = new Piece(PieceType.Horsey, GameColor.White),
    [new AlgebraicPoint("c1")] = new Piece(PieceType.Knook, GameColor.White),
    [new AlgebraicPoint("d1")] = new Piece(PieceType.Bishop, GameColor.White),
    [new AlgebraicPoint("e1")] = new Piece(PieceType.Queen, GameColor.White),
    [new AlgebraicPoint("f1")] = new Piece(PieceType.King, GameColor.White),
    [new AlgebraicPoint("g1")] = new Piece(PieceType.Bishop, GameColor.White),
    [new AlgebraicPoint("h1")] = new Piece(PieceType.Checker, GameColor.White),
    [new AlgebraicPoint("i1")] = new Piece(PieceType.Antiqueen, GameColor.White),
    [new AlgebraicPoint("j1")] = new Piece(PieceType.Rook, GameColor.White),

    [new AlgebraicPoint("a2")] = new Piece(PieceType.Pawn, GameColor.White),
    [new AlgebraicPoint("b2")] = new Piece(PieceType.Pawn, GameColor.White),
    [new AlgebraicPoint("c2")] = new Piece(PieceType.Pawn, GameColor.White),
    [new AlgebraicPoint("d2")] = new Piece(PieceType.UnderagePawn, GameColor.White),
    [new AlgebraicPoint("e2")] = new Piece(PieceType.Pawn, GameColor.White),
    [new AlgebraicPoint("f2")] = new Piece(PieceType.Pawn, GameColor.White),
    [new AlgebraicPoint("g2")] = new Piece(PieceType.UnderagePawn, GameColor.White),
    [new AlgebraicPoint("h2")] = new Piece(PieceType.Pawn, GameColor.White),
    [new AlgebraicPoint("i2")] = new Piece(PieceType.Pawn, GameColor.White),
    [new AlgebraicPoint("j2")] = new Piece(PieceType.Pawn, GameColor.White),
    #endregion

    [new AlgebraicPoint("a7")] = new Piece(PieceType.TraitorRook, Color: null),
    [new AlgebraicPoint("j4")] = new Piece(PieceType.TraitorRook, Color: null),

    #region Black Pieces
    [new AlgebraicPoint("a9")] = new Piece(PieceType.Pawn, GameColor.Black),
    [new AlgebraicPoint("b9")] = new Piece(PieceType.Pawn, GameColor.Black),
    [new AlgebraicPoint("c9")] = new Piece(PieceType.Pawn, GameColor.Black),
    [new AlgebraicPoint("d9")] = new Piece(PieceType.UnderagePawn, GameColor.Black),
    [new AlgebraicPoint("e9")] = new Piece(PieceType.Pawn, GameColor.Black),
    [new AlgebraicPoint("f9")] = new Piece(PieceType.Pawn, GameColor.Black),
    [new AlgebraicPoint("g9")] = new Piece(PieceType.UnderagePawn, GameColor.Black),
    [new AlgebraicPoint("h9")] = new Piece(PieceType.Pawn, GameColor.Black),
    [new AlgebraicPoint("i9")] = new Piece(PieceType.Pawn, GameColor.Black),
    [new AlgebraicPoint("j9")] = new Piece(PieceType.Pawn, GameColor.Black),

    [new AlgebraicPoint("a10")] = new Piece(PieceType.Rook, GameColor.Black),
    [new AlgebraicPoint("b10")] = new Piece(PieceType.Horsey, GameColor.Black),
    [new AlgebraicPoint("c10")] = new Piece(PieceType.Knook, GameColor.Black),
    [new AlgebraicPoint("d10")] = new Piece(PieceType.Bishop, GameColor.Black),
    [new AlgebraicPoint("e10")] = new Piece(PieceType.Queen, GameColor.Black),
    [new AlgebraicPoint("f10")] = new Piece(PieceType.King, GameColor.Black),
    [new AlgebraicPoint("g10")] = new Piece(PieceType.Bishop, GameColor.Black),
    [new AlgebraicPoint("h10")] = new Piece(PieceType.Checker, GameColor.Black),
    [new AlgebraicPoint("i10")] = new Piece(PieceType.Antiqueen, GameColor.Black),
    [new AlgebraicPoint("j10")] = new Piece(PieceType.Rook, GameColor.Black),
    #endregion
};

//FenDecoder fenDecoder = new(new PieceLetterMap(), Options.Create(new JsonOptions()));
//var chessBoard = fenDecoder.DecodeFen(
//    "r5rk2/p1pdb2ppp/1ph2cdaq1/+2b1n4/4p5/P3P5/1P3PBP1+/3D1CD3/2PH1Q2PP/R1N2KB2R {\"movedPieces\":[\"a5\",\"b4\",\"e5\",\"f4\",\"h4\",\"b8\",\"e6\",\"d3\",\"g3\",\"g8\",\"h10\",\"g10\"],\"lastMove\":{\"from\":\"d8\",\"to\":\"f7\",\"captures\":[{\"piece\":{\"type\":7,\"color\":0,\"hasMoved\":true},\"pos\":\"f7\"}]}}"
//);
//BitBoard board = BitBoard.FromPieces(chessBoard.Value.EnumeratePieces().ToDictionary());

BitBoard board = BitBoard.FromPieces(startingPosition);

int moveCount = 0;
new BitMoveGenerator().Generate(board, stackalloc BitMove[256], ref moveCount);

Stopwatch stopwatch = Stopwatch.StartNew();

while (true)
{
    UInt128 whiteBefore = board.WhitePieces;
    UInt128 blackBefore = board.BlackPieces;

    stopwatch.Restart();
    var bestMove = engine.FindBestMove(board, depth: 8).BestMove;
    stopwatch.Stop();

    Console.WriteLine(
        $"WHITE PIECES: {board.WhitePieces}, BLACK PIECES: {board.BlackPieces}. TIME TAKEN: {stopwatch.Elapsed}"
    );
    if (board.WhitePieces != whiteBefore || board.BlackPieces != blackBefore)
    {
        throw new Exception(
            $"WEE WOO WHITE BEFORE: {whiteBefore}, WHITE AFTER: {board.WhitePieces}, BLACK BEFORE: {blackBefore}, BLACK AFTER: {board.BlackPieces}"
        );
    }

    if (bestMove is not BitMove move)
    {
        break;
    }

    board.MakeMove(move);

    int fromY = move.From / 10;
    int fromX = move.From % 10;
    AlgebraicPoint from = new(fromX, fromY);

    int toY = move.To / 10;
    int toX = move.To % 10;
    AlgebraicPoint to = new(toX, toY);

    Console.WriteLine(
        $"{from} -> {to}, special move type: {move.SpecialMoveType}, stunned: {board.StunnedPieces}"
    );

    BitMove newMove;
    while (true)
    {
        Console.Write("From: ");
        string newFromStr = Console.ReadLine()!;
        if (newFromStr == "AAAA")
        {
            break;
        }
        if (!AlgebraicPoint.TryParse(newFromStr, maxWidth: 10, maxHeight: 10, out var newFrom))
        {
            continue;
        }
        Console.Write("To: ");
        if (
            !AlgebraicPoint.TryParse(
                Console.ReadLine()!,
                maxWidth: 10,
                maxHeight: 10,
                out var newTo
            )
        )
        {
            continue;
        }

        if (!board.TryGetPieceAt(newFrom.AsIdx(), out var movingPiece))
        {
            continue;
        }

        newMove = new()
        {
            From = newFrom.AsIdx(),
            To = newTo.AsIdx(),
            Piece = movingPiece.Value,
        };

        if ((board.Occupancy & (UInt128.One << newTo.AsIdx())) != 0)
        {
            newMove.CapturesMask |= UInt128.One << newTo.AsIdx();
        }
        board.MakeMove(newMove);
        break;
    }
}
