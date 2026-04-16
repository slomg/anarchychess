using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Ai;
using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.EngineShared;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Bots;

public readonly record struct CandidateBotMove(
    MoveEvaluation MoveEval,
    bool IsHang,
    bool IsCapturingHanging,
    bool IsRecapture,
    bool CausesForcedMove,
    bool IsMultiStep,
    int PlayabilityEval
);

public class LobotomizedAnarchyBot(
    IBotService botService,
    IRandomProvider randomProvider,
    IBotHeuristics botHeuristics,
    IBitMoveGenerator bitMoveGenerator
) : IBot
{
    public static readonly UserId BotId = "bot:lobotomized-anarchybot";

    private const int Depth = 4;
    private const double OpeningTemperature = 10;
    private const double MiddleGameTemperature = 25;
    private const double EndGameTemperature = 10;

    private const int TacticalThreshold = 100;
    private const int DrasticallyBadMoveThreshold = -200;

    private const double BlunderChance = 0.08;
    private const double TacticChance = 0.15;
    private const double TacticChancePerMoveBonus = 0.01;
    private const double SimpleTacticChance = 0.9;
    private const double CheckmateChance = 0.1;

    private const int HangPenalty = 300;
    private const int OpponentHangBonus = 50;
    private const int CausesForcedMovePenalty = 300;
    private const int MultiStepMovePenalty = 300;
    private const int LosesRookCastlingRightPenalty = 20;
    private const int LosesKingCastlingRightPenalty = 300;
    private const int BackwardsPenalty = 30;
    private const int EdgePenalty = 30;
    private const int BetaDecayPenalty = 300;
    private const int NonCentralPawnPenaltyInOpening = 20;
    private const int CastleBonus = 300;
    private const int SamePiecePenalty = 50;

    public BotType Type => BotType.LobotomizedAnarchyBot;

    private readonly IBotService _botService = botService;
    private readonly IRandomProvider _randomProvider = randomProvider;
    private readonly IBotHeuristics _botHeuristics = botHeuristics;
    private readonly IBitMoveGenerator _bitMoveGenerator = bitMoveGenerator;

    /// <summary>
    /// Selects the next move by following this process:
    /// 1. It identifies checkmates a human might miss and plays one probablistically
    /// 2. If not, it identifies tactical moves and plays one probablistically
    /// 3. If not, it seperates the non tactic moves into blunders and non blunders
    /// 4. From the non blunders, it selects moves deemed obvious and plays one of available
    /// 5. If not, it finds blunders deemed non obvious and plays one probablistically
    /// 6. If not, it attempts to play a non blunder
    /// 7. If there are none, it plays a tactic
    /// 8. If there are none, it plays a blunder
    /// 9. if there are none, it plays a non tactic
    /// </summary>
    public async Task<ErrorOr<MoveEvaluation>> FindMoveAsync(
        IReadOnlyChessBoard board,
        int lastEval,
        CancellationToken token = default
    )
    {
        var evaluationResult = await _botService.EvaluateAllMovesAsync(board, depth: Depth, token);
        if (evaluationResult.IsError)
        {
            return evaluationResult.Errors;
        }

        BitBoard bitboard = _botService.ConvertBoardToBit(board);
        float endgameFactor = EndgameFactorCalculator.EndgameFactor(bitboard);

        List<CandidateBotMove> moves =
        [
            .. evaluationResult.Value.Select(
                (move, i) => ScoreMove(move, board, bitboard, endgameFactor)
            ),
        ];

        (List<CandidateBotMove> missableCheckmates, List<CandidateBotMove> nonCheckmates) =
            OrderInto(
                moves,
                move =>
                {
                    bool isCheckmate = move.MoveEval.EvalForBot >= 100_000;
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
            return Softmax(missableCheckmates, board, endgameFactor);
        }

        (List<CandidateBotMove> tactics, List<CandidateBotMove> nonTactics) = OrderInto(
            nonCheckmates,
            move =>
                move.MoveEval.EvalForBot - lastEval > TacticalThreshold
                && (!move.IsCapturingHanging || move.CausesForcedMove || move.IsMultiStep)
        );
        if (tactics.Count == moves.Count)
        {
            return SoftmaxTactics(tactics, board, endgameFactor);
        }

        if (TrySoftmaxTactics(tactics, board, endgameFactor, out var tactic))
        {
            return tactic;
        }

        (List<CandidateBotMove> nonBlunders, List<CandidateBotMove> blunders) = OrderInto(
            nonTactics,
            move =>
                move.MoveEval.EvalForBot - lastEval > DrasticallyBadMoveThreshold && !move.IsHang
        );

        List<CandidateBotMove> obviousMoves =
        [
            .. nonBlunders.Where(move =>
                (move.IsCapturingHanging || move.IsRecapture)
                && !move.CausesForcedMove
                && !move.IsMultiStep
            ),
        ];
        if (obviousMoves.Count > 0)
        {
            return Softmax(obviousMoves, board, endgameFactor);
        }

        List<CandidateBotMove> missableBlunders = [.. blunders.Where(move => !move.IsHang)];
        if (
            board.Moves.Count > 10
            && missableBlunders.Count > 0
            && _randomProvider.NextDouble() < BlunderChance
        )
        {
            return Softmax(missableBlunders, board, endgameFactor);
        }

        if (nonBlunders.Count > 0)
        {
            return Softmax(nonBlunders, board, endgameFactor);
        }
        else if (tactics.Count > 0)
        {
            return SoftmaxTactics(tactics, board, endgameFactor);
        }
        else if (missableBlunders.Count > 0)
        {
            return Softmax(missableBlunders, board, endgameFactor);
        }
        else
        {
            return Softmax(nonTactics, board, endgameFactor);
        }
    }

    private CandidateBotMove ScoreMove(
        MoveEvaluation moveEval,
        IReadOnlyChessBoard board,
        BitBoard bitboard,
        float endgameFactor
    )
    {
        BitBoard bitboardAfterMove = new(bitboard);
        bitboardAfterMove.MakeMove(moveEval.Move);

        BitMove[] opponentMoves = new BitMove[EngineConstants.MaxMoves];
        int opponentMoveCount = 0;
        _bitMoveGenerator.Generate(bitboardAfterMove, opponentMoves, ref opponentMoveCount);

        BotHeuristicContext context = new(
            Board: board,
            Bitboard: bitboard,
            BitboardAfterMove: bitboardAfterMove,
            OpponentMoves: opponentMoves,
            OpponentMoveCount: opponentMoveCount
        );

        bool isMultiStep = _botHeuristics.IsMultiStep(moveEval.Move, context);
        bool isHang = _botHeuristics.IsHang(moveEval.Move, context);
        bool causesForcedMove = _botHeuristics.CausesForcedMove(moveEval.Move, context);
        bool isCapturingHanging = _botHeuristics.IsCapturingOpponentHang(moveEval.Move, context);
        bool isRecapture = _botHeuristics.IsRecapture(moveEval.Move, context);
        bool losesKingCastling = _botHeuristics.LosesKingCastlingRight(moveEval.Move, context);
        bool losesRookCastling = _botHeuristics.LosesRookCastlingRight(moveEval.Move, context);
        bool isBackwards = _botHeuristics.IsBackwards(moveEval.Move);
        bool isEdge = _botHeuristics.IsEdge(moveEval.Move);
        bool isNonCentralPawn = _botHeuristics.IsNonCentralPawn(moveEval.Move);
        bool isSamePieceAsLast = _botHeuristics.IsSameAsPieceAsLast(moveEval.Move, context);

        int playabilityEval = moveEval.EvalForBot;

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

        if (isMultiStep && !isRecapture)
        {
            playabilityEval -= MultiStepMovePenalty;
        }

        if (losesKingCastling)
        {
            playabilityEval -= (int)(LosesKingCastlingRightPenalty * (1 - endgameFactor));
        }

        if (losesRookCastling)
        {
            playabilityEval -= (int)(LosesRookCastlingRightPenalty * (1 - endgameFactor));
        }

        if (isBackwards)
        {
            playabilityEval -= BackwardsPenalty;
        }

        if (isEdge)
        {
            playabilityEval -= EdgePenalty;
        }

        if (moveEval.Move.SpecialMoveType is SpecialMoveType.RadioactiveBetaDecay)
        {
            playabilityEval -= BetaDecayPenalty;
        }

        if (
            moveEval.Move.SpecialMoveType is SpecialMoveType.KingsideCastle
            || moveEval.Move.SpecialMoveType is SpecialMoveType.QueensideCastle
        )
        {
            playabilityEval += CastleBonus;
        }

        if (isNonCentralPawn && board.Moves.Count <= 20)
        {
            playabilityEval -= NonCentralPawnPenaltyInOpening;
        }

        if (isSamePieceAsLast)
        {
            playabilityEval += SamePiecePenalty;
        }

        return new CandidateBotMove(
            moveEval,
            IsHang: isHang,
            IsCapturingHanging: isCapturingHanging,
            IsRecapture: isRecapture,
            CausesForcedMove: causesForcedMove,
            IsMultiStep: isMultiStep,
            PlayabilityEval: playabilityEval
        );
    }

    private static (List<T> match, List<T> fail) OrderInto<T>(
        IEnumerable<T> source,
        Func<T, bool> predicate
    )
    {
        List<T> match = [];
        List<T> fail = [];

        foreach (var item in source)
        {
            if (predicate(item))
            {
                match.Add(item);
            }
            else
            {
                fail.Add(item);
            }
        }

        return (match, fail);
    }

    private MoveEvaluation Softmax(
        List<CandidateBotMove> moves,
        IReadOnlyChessBoard board,
        float endgameFactor
    )
    {
        double temperature = GetTemperature(board, endgameFactor);
        var result = _randomProvider.Softmax(moves, x => x.PlayabilityEval, temperature).MoveEval;
        return result;
    }

    private static double GetTemperature(IReadOnlyChessBoard board, float endgameFactor)
    {
        if (board.Moves.Count <= 20)
        {
            return OpeningTemperature;
        }

        double temperature =
            (MiddleGameTemperature * (1 - endgameFactor)) + (EndGameTemperature * endgameFactor);
        return temperature;
    }

    private bool TrySoftmaxTactics(
        List<CandidateBotMove> tactics,
        IReadOnlyChessBoard board,
        float endgameFactor,
        [NotNullWhen(true)] out MoveEvaluation? result
    )
    {
        result = null;
        if (tactics.Count == 0)
        {
            return false;
        }

        if (
            _randomProvider.NextDouble()
            < Math.Min(1, TacticChance + TacticChancePerMoveBonus * tactics.Count)
        )
        {
            return false;
        }

        var (complexTactics, simpleTactics) = SortTactics(tactics);
        if (complexTactics.Count == 0)
        {
            result = Softmax(simpleTactics, board, endgameFactor);
            return true;
        }

        bool playSimple = _randomProvider.NextDouble() < SimpleTacticChance;
        if (simpleTactics.Count == 0 && playSimple)
        {
            return false;
        }

        if (playSimple)
        {
            result = Softmax(simpleTactics, board, endgameFactor);
            return true;
        }
        else
        {
            result = Softmax(complexTactics, board, endgameFactor);
            return true;
        }
    }

    private MoveEvaluation SoftmaxTactics(
        List<CandidateBotMove> tactics,
        IReadOnlyChessBoard board,
        float endgameFactor
    )
    {
        var (complexTactics, simpleTactics) = SortTactics(tactics);
        bool playSimple = _randomProvider.NextDouble() < SimpleTacticChance;

        if (playSimple && simpleTactics.Count == 0)
        {
            return Softmax(complexTactics, board, endgameFactor);
        }

        return playSimple
            ? Softmax(simpleTactics, board, endgameFactor)
            : Softmax(complexTactics, board, endgameFactor);
    }

    private static (List<CandidateBotMove> complex, List<CandidateBotMove> simple) SortTactics(
        List<CandidateBotMove> tactics
    )
    {
        List<CandidateBotMove> complexTactics = [];
        List<CandidateBotMove> simpleTactics = [];
        foreach (CandidateBotMove move in tactics)
        {
            if (
                move.CausesForcedMove
                || move.IsMultiStep
                || move.IsHang
                || move.MoveEval.Move.SpecialMoveType is SpecialMoveType.Throw
            )
            {
                complexTactics.Add(move);
            }
            else
            {
                simpleTactics.Add(move);
            }
        }
        return (complexTactics, simpleTactics);
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
