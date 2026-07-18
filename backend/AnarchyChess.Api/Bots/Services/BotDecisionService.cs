using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Ai;
using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Ai.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.EngineShared;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Services;

public readonly record struct CandidateBotMove(
    MoveEvaluation MoveEval,
    bool IsHang,
    bool IsCapturingHanging,
    bool IsRecapture,
    bool CausesForcedMove,
    bool IsMultiStep,
    int PlayabilityEval
);

public enum BotMoveCategory
{
    NonBlunder,
    MissableBlunder,
    Tactic,
    NonTactic,
}

public record BotBehaviorProfile(
    int Depth,
    double OpeningTemperature,
    double MiddleGameTemperature,
    double EndGameTemperature,
    int TacticalThreshold,
    int BlunderThreshold,
    double BlunderChance,
    double TacticChance,
    double TacticChancePerMoveBonus,
    double SimpleTacticChance,
    double CheckmateChance,
    int HangPenalty,
    int OpponentHangBonus,
    int CausesForcedMovePenalty,
    int MultiStepMovePenalty,
    int LosesRookCastlingRightPenalty,
    int LosesKingCastlingRightPenalty,
    int BackwardsPenalty,
    int EdgePenalty,
    int BetaDecayPenalty,
    int NonCentralPawnPenaltyInOpening,
    int CastleBonus,
    int SamePiecePenalty,
    int ThrowPenalty,
    IReadOnlyList<BotMoveCategory> FinalDecisionOrder,
    Func<CandidateBotMove, bool> ObviousMovePredicate,
    Func<CandidateBotMove, bool>? MoveFilter = null
);

public interface IBotDecisionService
{
    Task<ErrorOr<MoveEvaluation>> DecideMoveAsync(
        IReadOnlyChessBoard board,
        int lastEval,
        CancellationToken token = default
    );
}

public class BotDecisionService(
    IBotService botService,
    IRandomProvider randomProvider,
    IBotHeuristics botHeuristics,
    ILogger<BotDecisionService> logger,
    BotBehaviorProfile behaviorProfile
) : IBotDecisionService
{
    private readonly IBotService _botService = botService;
    private readonly IRandomProvider _randomProvider = randomProvider;
    private readonly IBotHeuristics _botHeuristics = botHeuristics;
    private readonly ILogger<BotDecisionService> _logger = logger;

    private readonly BotBehaviorProfile _behaviorProfile = behaviorProfile;

    /// <summary>
    /// Selects the next move by following this process:
    /// 1. It identifies checkmates a human might miss and plays one probablistically
    /// 2. If not, it identifies tactical moves and plays one probablistically
    /// 3. If not, it seperates the non tactic moves into blunders and non blunders
    /// 4. From the non blunders, it selects moves deemed obvious and plays one of available
    /// 5. If not, it finds blunders deemed non obvious and plays one probablistically
    /// 6. If not, it plays moves by category following <see cref="BotBehaviorProfile.FinalDecisionOrder"/>
    /// </summary>
    public async Task<ErrorOr<MoveEvaluation>> DecideMoveAsync(
        IReadOnlyChessBoard board,
        int lastEval,
        CancellationToken token = default
    )
    {
        _logger.LogInformation("LAST EVAL: {LastEval}", lastEval);
        var evaluationResult = await _botService.EvaluateAllMovesAsync(
            board,
            depth: _behaviorProfile.Depth,
            token
        );
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
        moves = ApplyMoveFilter(moves);

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
        if (
            missableCheckmates.Count > 0
            && _randomProvider.NextDouble() > _behaviorProfile.CheckmateChance
        )
        {
            _logger.LogInformation("Playing missable checkmate");
            return Softmax(missableCheckmates, board, endgameFactor);
        }

        (List<CandidateBotMove> tactics, List<CandidateBotMove> nonTactics) = OrderInto(
            nonCheckmates,
            move =>
                move.MoveEval.EvalForBot - lastEval > _behaviorProfile.TacticalThreshold
                && (!move.IsCapturingHanging || move.CausesForcedMove || move.IsMultiStep)
        );
        if (tactics.Count == moves.Count)
        {
            _logger.LogInformation("Playing tactic because all moves are tactics");
            return SoftmaxTactics(tactics, board, endgameFactor);
        }

        if (TrySoftmaxTactics(tactics, board, endgameFactor, out var tactic))
        {
            return tactic;
        }

        (List<CandidateBotMove> nonBlunders, List<CandidateBotMove> blunders) = OrderInto(
            nonTactics,
            move =>
                move.MoveEval.EvalForBot - lastEval > _behaviorProfile.BlunderThreshold
                && !move.IsHang
        );

        List<CandidateBotMove> obviousMoves =
        [
            .. nonBlunders.Where(move => _behaviorProfile.ObviousMovePredicate(move)),
        ];
        if (obviousMoves.Count > 0)
        {
            _logger.LogInformation("Playing obvious move");
            return Softmax(obviousMoves, board, endgameFactor);
        }

        List<CandidateBotMove> missableBlunders = [.. blunders.Where(move => !move.IsHang)];
        if (
            board.Moves.Count > 10
            && missableBlunders.Count > 0
            && _randomProvider.NextDouble() > _behaviorProfile.BlunderChance
        )
        {
            _logger.LogInformation("Playing missable blunder");
            return Softmax(missableBlunders, board, endgameFactor);
        }

        Dictionary<BotMoveCategory, List<CandidateBotMove>> categoryMap = new()
        {
            [BotMoveCategory.NonBlunder] = nonBlunders,
            [BotMoveCategory.MissableBlunder] = missableBlunders,
            [BotMoveCategory.Tactic] = tactics,
            [BotMoveCategory.NonTactic] = nonTactics,
        };

        foreach (var category in _behaviorProfile.FinalDecisionOrder)
        {
            List<CandidateBotMove> categoryMoves = categoryMap[category];
            if (categoryMoves.Count == 0)
            {
                continue;
            }

            _logger.LogInformation("Playing {Category} from final decision order", category);
            return category switch
            {
                BotMoveCategory.Tactic => SoftmaxTactics(categoryMoves, board, endgameFactor),
                _ => Softmax(categoryMoves, board, endgameFactor),
            };
        }

        _logger.LogInformation("Shouldn't happen: softmax all moves");
        return Softmax(moves, board, endgameFactor);
    }

    private List<CandidateBotMove> ApplyMoveFilter(List<CandidateBotMove> moves)
    {
        var filter = _behaviorProfile.MoveFilter;
        if (filter is null)
        {
            return moves;
        }

        List<CandidateBotMove> filtered = [.. moves.Where(move => filter(move))];
        return filtered.Count > 0 ? filtered : moves;
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
        BitMoveGenerator.Generate(bitboardAfterMove, opponentMoves, ref opponentMoveCount);

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
            playabilityEval -= _behaviorProfile.HangPenalty;
        }

        if (isCapturingHanging && !causesForcedMove)
        {
            playabilityEval += _behaviorProfile.OpponentHangBonus;
        }

        if (causesForcedMove)
        {
            playabilityEval -= _behaviorProfile.CausesForcedMovePenalty;
        }

        if (isMultiStep && !isRecapture)
        {
            playabilityEval -= _behaviorProfile.MultiStepMovePenalty;
        }

        if (losesKingCastling)
        {
            playabilityEval -= (int)(
                _behaviorProfile.LosesKingCastlingRightPenalty * (1 - endgameFactor)
            );
        }

        if (losesRookCastling)
        {
            playabilityEval -= (int)(
                _behaviorProfile.LosesRookCastlingRightPenalty * (1 - endgameFactor)
            );
        }

        if (isBackwards)
        {
            playabilityEval -= _behaviorProfile.BackwardsPenalty;
        }

        if (isEdge)
        {
            playabilityEval -= _behaviorProfile.EdgePenalty;
        }

        if (moveEval.Move.SpecialMoveType is SpecialMoveType.RadioactiveBetaDecay)
        {
            playabilityEval -= _behaviorProfile.BetaDecayPenalty;
        }

        if (
            moveEval.Move.SpecialMoveType is SpecialMoveType.KingsideCastle
            || moveEval.Move.SpecialMoveType is SpecialMoveType.QueensideCastle
        )
        {
            playabilityEval += _behaviorProfile.CastleBonus;
        }

        if (isNonCentralPawn && board.Moves.Count <= 20)
        {
            playabilityEval -= _behaviorProfile.NonCentralPawnPenaltyInOpening;
        }

        if (isSamePieceAsLast)
        {
            playabilityEval += _behaviorProfile.SamePiecePenalty;
        }

        if (moveEval.Move.SpecialMoveType is SpecialMoveType.Throw)
        {
            playabilityEval -= _behaviorProfile.ThrowPenalty;
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

    private double GetTemperature(IReadOnlyChessBoard board, float endgameFactor)
    {
        if (board.Moves.Count <= 20)
        {
            return _behaviorProfile.OpeningTemperature;
        }

        double temperature =
            (_behaviorProfile.MiddleGameTemperature * (1 - endgameFactor))
            + (_behaviorProfile.EndGameTemperature * endgameFactor);
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
            > Math.Min(
                1,
                _behaviorProfile.TacticChance
                    + _behaviorProfile.TacticChancePerMoveBonus * tactics.Count
            )
        )
        {
            return false;
        }

        var (complexTactics, simpleTactics) = SortTactics(tactics);
        if (complexTactics.Count == 0)
        {
            _logger.LogInformation("Playing simple tactic");
            result = Softmax(simpleTactics, board, endgameFactor);
            return true;
        }

        bool playSimple = _randomProvider.NextDouble() > _behaviorProfile.SimpleTacticChance;
        if (simpleTactics.Count == 0 && playSimple)
        {
            return false;
        }

        if (playSimple)
        {
            _logger.LogInformation("Playing simple tactic");
            result = Softmax(simpleTactics, board, endgameFactor);
            return true;
        }
        else
        {
            _logger.LogInformation("Playing complex tactic");
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
        bool playSimple = _randomProvider.NextDouble() > _behaviorProfile.SimpleTacticChance;

        if (playSimple && simpleTactics.Count == 0)
        {
            _logger.LogInformation("Playing complex tactic beacuse no simple tactics");
            return Softmax(complexTactics, board, endgameFactor);
        }

        if (playSimple)
        {
            _logger.LogInformation("Playing simple tactic");
        }
        else
        {
            _logger.LogInformation("Playing complex tactic");
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
}
