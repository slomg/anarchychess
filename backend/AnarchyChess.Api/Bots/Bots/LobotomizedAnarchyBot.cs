using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.EngineShared;
using ErrorOr;
using Microsoft.CodeAnalysis;

namespace AnarchyChess.Api.Bots.Bots;

public readonly record struct CandidateBotMove(
    AiEngineMove Move,
    Move BoardMove,
    bool IsHang,
    bool IsCapturingHanging,
    bool IsRecapture,
    bool CausesForcedMove,
    int PlayabilityEval
);

public class LobotomizedAnarchyBot(
    IBotService botService,
    IRandomProvider randomProvider,
    ILegalMoveCalculator legalMoveCalculator
) : IBot
{
    public static readonly UserId BotId = "bot:lobotomized-anarchybot";

    private const int Depth = 6;
    private const double OpeningTemperature = 5;
    private const double Temperature = 20;

    private const int TacticalThreshold = 150;
    private const int DrasticallyBadMoveThreshold = -200;

    private const double BlunderChance = 0.08;
    private const double TacticChance = 0.15;
    private const double TacticChancePerMoveBonus = 0.01;
    private const double CheckmateChance = 0.3;

    private const int PawnPenaltyForLanding = 100;
    private const int HangPenalty = 300;
    private const int OpponentHangBonus = 100;
    private const int CausesForcedMovePenalty = 300;
    private const int MultiStepMovePenalty = 300;

    public BotType Type => BotType.LobotomizedAnarchyBot;

    private readonly IBotService _botService = botService;
    private readonly IRandomProvider _randomProvider = randomProvider;
    private readonly ILegalMoveCalculator _legalMoveCalculator = legalMoveCalculator;

    public async Task<ErrorOr<AiEngineMove>> FindMoveAsync(
        IReadOnlyChessBoard board,
        int lastEval,
        LegalMoveSet legalMoves,
        CancellationToken token = default
    )
    {
        var evaluationResult = await _botService.EvaluateAllMovesAsync(board, depth: Depth, token);
        if (evaluationResult.IsError)
        {
            return evaluationResult.Errors;
        }

        var depth2EvaluationResult = await _botService.EvaluateAllMovesAsync(
            board,
            depth: 2,
            token
        );
        if (depth2EvaluationResult.IsError)
        {
            return depth2EvaluationResult.Errors;
        }

        List<CandidateBotMove> moves =
        [
            .. evaluationResult
                .Value.Select(
                    (move, i) =>
                        ScoreMove(
                            move,
                            lastEval: lastEval,
                            depth2Eval: depth2EvaluationResult.Value[i].EvalForBot,
                            legalMoves,
                            board
                        )
                )
                .OrderByDescending(x => x.Move.EvalForBot),
        ];

        (List<CandidateBotMove> missableCheckmates, List<CandidateBotMove> nonCheckmates) =
            TakeWhilePrefix(
                moves,
                move =>
                {
                    bool isCheckmate = move.Move.EvalForBot >= 100_000;
                    if (!isCheckmate)
                    {
                        return false;
                    }

                    if (move.CausesForcedMove)
                    {
                        return true;
                    }
                    if (!move.IsRecapture && !move.IsCapturingHanging)
                    {
                        return true;
                    }

                    return false;
                }
            );
        if (missableCheckmates.Count > 0 && _randomProvider.NextDouble() < CheckmateChance)
        {
            return Softmax(missableCheckmates, board);
        }

        (List<CandidateBotMove> tactics, List<CandidateBotMove> nonTactics) = TakeWhilePrefix(
            nonCheckmates,
            move =>
                move.Move.EvalForBot - lastEval > TacticalThreshold
                && (
                    !move.IsCapturingHanging
                    || move.CausesForcedMove
                    || move.BoardMove.IntermediateSquares.Count > 0
                )
        );
        if (tactics.Count == moves.Count)
        {
            return Softmax(moves, board);
        }

        if (tactics.Count > 0 && _randomProvider.NextDouble() < CalcTacticChance(tactics.Count))
        {
            return Softmax(tactics, board);
        }

        (List<CandidateBotMove> nonBlunders, List<CandidateBotMove> blunders) = TakeWhilePrefix(
            nonTactics,
            move => move.Move.EvalForBot - lastEval > DrasticallyBadMoveThreshold
        );
        if (nonBlunders.Count == 0 && tactics.Count > 0)
        {
            return Softmax(tactics, board);
        }

        int nonBlunderRecapturesCount = 0;
        foreach (var move in nonBlunders)
        {
            if (move.IsRecapture)
            {
                nonBlunderRecapturesCount++;
            }
            else
            {
                break;
            }
        }

        if (nonBlunders.Count > 0 && nonBlunderRecapturesCount == nonBlunders.Count)
        {
            return Softmax(nonBlunders, board);
        }

        List<CandidateBotMove> nonStupidBlunders = [.. blunders.Where(move => !move.IsHang)];
        if (
            board.Moves.Count > 10
            && nonStupidBlunders.Count > 0
            && _randomProvider.NextDouble() < BlunderChance
        )
        {
            return Softmax(nonStupidBlunders, board);
        }

        return Softmax(nonTactics, board);
    }

    private CandidateBotMove ScoreMove(
        AiEngineMove move,
        int lastEval,
        int depth2Eval,
        LegalMoveSet legalMoves,
        IReadOnlyChessBoard board
    )
    {
        var boardMove =
            legalMoves.FindBotMove(move)
            ?? throw new ArgumentException("No board move found for engine move", nameof(move));

        bool isHang = IsHang(move, depth2Eval: depth2Eval, lastEval: lastEval);
        bool causesForcedMove = CausesForcedMove(boardMove, board);
        bool isCapturingHanging = IsCapturingOpponentHang(
            move,
            depth2Eval: depth2Eval,
            lastEval: lastEval
        );
        bool isRecapture =
            board.Moves.Count > 0
            && board.Moves[^1].Captures.Count > 0
            && move.To == board.Moves[^1].To;

        int playabilityEval = move.EvalForBot;
        if (isHang)
        {
            playabilityEval -= HangPenalty;
        }

        if (isCapturingHanging && !causesForcedMove)
        {
            playabilityEval += OpponentHangBonus;
        }

        if (causesForcedMove)
        {
            playabilityEval -= CausesForcedMovePenalty;
        }

        if (board.TryGetPieceAt(move.To, out var piece) && piece.Type is PieceType.Pawn)
        {
            playabilityEval -= PawnPenaltyForLanding;
        }

        if (boardMove.IntermediateSquares.Count > 0)
        {
            playabilityEval -= MultiStepMovePenalty;
        }

        return new CandidateBotMove(
            move,
            boardMove,
            IsHang: isHang,
            IsCapturingHanging: isCapturingHanging,
            IsRecapture: isRecapture,
            CausesForcedMove: causesForcedMove,
            PlayabilityEval: playabilityEval
        );
    }

    private static (List<T> head, List<T> tail) TakeWhilePrefix<T>(
        IEnumerable<T> source,
        Func<T, bool> predicate
    )
    {
        List<T> head = [];
        List<T> tail = [];
        bool inHead = true;

        foreach (var item in source)
        {
            if (inHead && predicate(item))
            {
                head.Add(item);
            }
            else
            {
                inHead = false;
                tail.Add(item);
            }
        }

        return (head, tail);
    }

    private AiEngineMove Softmax(List<CandidateBotMove> moves, IReadOnlyChessBoard board)
    {
        double temperature = board.Moves.Count <= 10 ? OpeningTemperature : Temperature;

        double max = moves.Max(x => x.PlayabilityEval);
        double[] expScores =
        [
            .. moves.Select(x => Math.Exp((x.PlayabilityEval - max) / temperature)),
        ];
        double sumExp = expScores.Sum();
        double[] probabilities = [.. expScores.Select(x => x / sumExp)];

        double threshold = _randomProvider.NextDouble();
        double cum = 0; // hehe
        int selectedIdx = moves.Count - 1;

        for (int i = 0; i < moves.Count; i++)
        {
            cum += probabilities[i];
            if (cum >= threshold)
            {
                selectedIdx = i;
                break;
            }
        }
        return moves[selectedIdx].Move;
    }

    private static double CalcTacticChance(int numOfTactics) =>
        Math.Min(1, TacticChance + TacticChancePerMoveBonus * numOfTactics);

    private static bool IsHang(AiEngineMove move, int depth2Eval, int lastEval)
    {
        int pieceValueMargin = 150;
        if (lastEval - depth2Eval < pieceValueMargin)
        {
            return false;
        }

        return lastEval - move.EvalForBot >= pieceValueMargin;
    }

    private bool CausesForcedMove(Move boardMove, IReadOnlyChessBoard board)
    {
        ChessBoard boardCopy = new(board);
        boardCopy.PlayMove(boardMove);
        return _legalMoveCalculator.HasForcedMoves(boardCopy);
    }

    private static bool IsCapturingOpponentHang(AiEngineMove move, int depth2Eval, int lastEval)
    {
        if (move.Captures is null || move.Captures.Count == 0)
        {
            return false;
        }

        if (depth2Eval >= 100_000)
        {
            return true;
        }

        int pieceValueMargin = 150;
        if (depth2Eval - lastEval < pieceValueMargin)
        {
            return false;
        }

        return move.EvalForBot - lastEval >= pieceValueMargin;
    }

    public GamePlayer CreateBotPlayer(GameColor color) =>
        new(
            UserId: BotId,
            Color: color,
            UserName: "Lobotomized Anarchy Bot",
            CountryCode: "FR",
            Rating: -161660
        );
}
