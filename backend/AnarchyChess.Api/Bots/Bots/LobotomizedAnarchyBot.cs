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
    ILogger<LobotomizedAnarchyBot> logger,
    IBotService botService,
    IRandomProvider randomProvider,
    IBotHeuristics botHeuristics,
    IBitMoveGenerator bitMoveGenerator
) : IBot
{
    public static readonly UserId BotId = "bot:lobotomized-anarchybot";

    private const int Depth = 6;
    private const double OpeningTemperature = 10;
    private const double MiddleGameTemperature = 30;
    private const double EndGameTemperature = 10;

    private const int TacticalThreshold = 150;
    private const int DrasticallyBadMoveThreshold = -200;

    private const double BlunderChance = 0.08;
    private const double TacticChance = 0.15;
    private const double TacticChancePerMoveBonus = 0.01;
    private const double SimpleTacticChance = 0.9;
    private const double CheckmateChance = 0.3;

    private const int HangPenalty = 300;
    private const int OpponentHangBonus = 100;
    private const int CausesForcedMovePenalty = 300;
    private const int MultiStepMovePenalty = 300;
    private const int LosesRookCastlingRightPenalty = 20;
    private const int LosesKingCastlingRightPenalty = 30;
    private const int BackwardsPenalty = 30;

    public BotType Type => BotType.LobotomizedAnarchyBot;

    private readonly ILogger<LobotomizedAnarchyBot> _logger = logger;
    private readonly IBotService _botService = botService;
    private readonly IRandomProvider _randomProvider = randomProvider;
    private readonly IBotHeuristics _botHeuristics = botHeuristics;
    private readonly IBitMoveGenerator _bitMoveGenerator = bitMoveGenerator;

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
            .. evaluationResult
                .Value.Select((move, i) => ScoreMove(move, board, bitboard, endgameFactor))
                .OrderByDescending(x => x.MoveEval.EvalForBot),
        ];

        (List<CandidateBotMove> missableCheckmates, List<CandidateBotMove> nonCheckmates) =
            TakeWhilePrefix(
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
            _logger.LogInformation("playing missable checkmate");
            return Softmax(missableCheckmates, board, endgameFactor);
        }

        (List<CandidateBotMove> tactics, List<CandidateBotMove> nonTactics) = TakeWhilePrefix(
            nonCheckmates,
            move =>
                move.MoveEval.EvalForBot - lastEval > TacticalThreshold
                && (!move.IsCapturingHanging || move.CausesForcedMove || move.IsMultiStep)
        );
        if (tactics.Count == moves.Count)
        {
            _logger.LogInformation("playing tactic because no other move");
            return SoftmaxTactics(moves, board, endgameFactor);
        }

        if (TrySoftmaxTactics(tactics, board, endgameFactor, out var tactic))
        {
            _logger.LogInformation("playing tactic");
            return tactic;
        }

        (List<CandidateBotMove> nonBlunders, List<CandidateBotMove> blunders) = TakeWhilePrefix(
            nonTactics,
            move => move.MoveEval.EvalForBot - lastEval > DrasticallyBadMoveThreshold
        );
        if (nonBlunders.Count == 0 && tactics.Count > 0)
        {
            _logger.LogInformation("playing tactic because no non blunders");
            return SoftmaxTactics(tactics, board, endgameFactor);
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
            _logger.LogInformation("playing non blunder");
            return Softmax(nonBlunders, board, endgameFactor);
        }

        List<CandidateBotMove> nonStupidBlunders = [.. blunders.Where(move => !move.IsHang)];
        if (
            board.Moves.Count > 10
            && nonStupidBlunders.Count > 0
            && _randomProvider.NextDouble() < BlunderChance
        )
        {
            _logger.LogInformation("playing blunder");
            return Softmax(nonStupidBlunders, board, endgameFactor);
        }

        _logger.LogInformation("playing non tactic");
        return Softmax(nonTactics, board, endgameFactor);
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

        _logger.LogInformation(
            "from: {From}, to: {To}, candidate: {Candidate}, is backwards: {IsBackwards}",
            AlgebraicPoint.FromIdx(moveEval.Move.From),
            AlgebraicPoint.FromIdx(moveEval.Move.To),
            new CandidateBotMove(
                moveEval,
                IsHang: isHang,
                IsCapturingHanging: isCapturingHanging,
                IsRecapture: isRecapture,
                CausesForcedMove: causesForcedMove,
                IsMultiStep: isMultiStep,
                PlayabilityEval: playabilityEval
            ),
            isBackwards
        );

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

    private MoveEvaluation Softmax(
        List<CandidateBotMove> moves,
        IReadOnlyChessBoard board,
        float endgameFactor
    )
    {
        double temperature = GetTemperature(board, endgameFactor);
        var result = _randomProvider.Softmax(moves, x => x.PlayabilityEval, temperature).MoveEval;
        _logger.LogInformation(
            "TEMPERATURE: {Tempterature}, FROM: {From}, TO: {To}",
            temperature,
            result.Move.From,
            result.Move.To
        );
        return result;
    }

    private static double GetTemperature(IReadOnlyChessBoard board, float endgameFactor)
    {
        if (board.Moves.Count <= 10)
        {
            return OpeningTemperature;
        }

        double temperature =
            (MiddleGameTemperature * endgameFactor) + (EndGameTemperature * (1 - endgameFactor));
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
            _logger.LogInformation("playing simple tactic");
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
            _logger.LogInformation("playing simple tactic");
            result = Softmax(simpleTactics, board, endgameFactor);
            return true;
        }
        else
        {
            _logger.LogInformation("playing complex tactic");
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

        if (playSimple)
        {
            _logger.LogInformation("playing simple tactic");
        }
        else
        {
            _logger.LogInformation("playing complex tactic");
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
            if (move.CausesForcedMove || move.IsMultiStep)
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
