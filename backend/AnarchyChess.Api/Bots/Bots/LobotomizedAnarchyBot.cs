using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.EngineShared;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Bots;

public class LobotomizedAnarchyBot(IBotDecisionServiceFactory botDecisionServiceFactory) : IBot
{
    public static readonly UserId BotId = "bot:lobotomized-anarchybot";

    public BotType Type => BotType.LobotomizedAnarchyBot;

    private readonly IBotDecisionService _botDecision = botDecisionServiceFactory.Create(
        new(
            Depth: 4,
            OpeningTemperature: 10,
            MiddleGameTemperature: 25,
            EndGameTemperature: 10,
            //
            TacticalThreshold: 100,
            BlunderThreshold: -200,
            //
            BlunderChance: 0.08,
            TacticChance: 0.15,
            TacticChancePerMoveBonus: 0.01,
            SimpleTacticChance: 0.9,
            CheckmateChance: 0.1,
            //
            HangPenalty: 300,
            OpponentHangBonus: 50,
            CausesForcedMovePenalty: 300,
            MultiStepMovePenalty: 300,
            LosesRookCastlingRightPenalty: 20,
            LosesKingCastlingRightPenalty: 300,
            BackwardsPenalty: 30,
            EdgePenalty: 30,
            BetaDecayPenalty: 300,
            NonCentralPawnPenaltyInOpening: 20,
            CastleBonus: 300,
            SamePiecePenalty: 50,
            ThrowPenalty: 0,
            //
            FinalDecisionOrder:
            [
                BotMoveCategory.NonBlunder,
                BotMoveCategory.Tactic,
                BotMoveCategory.MissableBlunder,
                BotMoveCategory.NonTactic,
            ],
            ObviousMovePredicate: move =>
                (move.IsCapturingHanging || move.IsRecapture)
                && !move.CausesForcedMove
                && !move.IsMultiStep
        )
    );

    public GamePlayer CreateBotPlayer(GameColor color) =>
        new(
            UserId: BotId,
            Color: color,
            UserName: "Lobotomized Anarchy Bot",
            CountryCode: "FR",
            Rating: -161660
        );

    public Task<ErrorOr<MoveEvaluation>> FindMoveAsync(
        IReadOnlyChessBoard board,
        int lastEval,
        CancellationToken token = default
    ) => _botDecision.DecideMoveAsync(board, lastEval, token);
}
