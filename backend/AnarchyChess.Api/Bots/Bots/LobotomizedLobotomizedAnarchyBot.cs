using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.EngineShared;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Bots;

public class LobotomizedLobotomizedAnarchyBot(IBotDecisionServiceFactory botDecisionServiceFactory)
    : IBot
{
    public static readonly UserId BotId = "bot:lobotomized-lobotomized-anarchybot";

    public BotType Type => BotType.LobotomizedLobotomizedAnarchyBot;

    private readonly IBotDecisionService _botDecision = botDecisionServiceFactory.Create(
        new(
            Depth: 4,
            OpeningTemperature: 10,
            MiddleGameTemperature: 80,
            EndGameTemperature: 50,
            //
            TacticalThreshold: 100,
            BlunderThreshold: -300,
            //
            BlunderChance: 0.3,
            TacticChance: 0,
            TacticChancePerMoveBonus: 0,
            SimpleTacticChance: 0,
            CheckmateChance: 0.05,
            //
            HangPenalty: -100,
            OpponentHangBonus: -100,
            CausesForcedMovePenalty: 500,
            MultiStepMovePenalty: 500,
            LosesRookCastlingRightPenalty: 50,
            LosesKingCastlingRightPenalty: 300,
            BackwardsPenalty: 30,
            EdgePenalty: 70,
            BetaDecayPenalty: 300,
            NonCentralPawnPenaltyInOpening: 1000,
            CastleBonus: 300,
            SamePiecePenalty: 70,
            ThrowPenalty: 400,
            //
            FinalDecisionOrder:
            [
                BotMoveCategory.MissableBlunder,
                BotMoveCategory.NonBlunder,
                BotMoveCategory.Tactic,
                BotMoveCategory.NonTactic,
            ],
            ObviousMovePredicate: move =>
                move.IsRecapture && !move.CausesForcedMove && !move.IsMultiStep,
            MoveFilter: move =>
                !move.IsMultiStep
                && move.MoveEval.Move.SpecialMoveType is not SpecialMoveType.Throw
                && move.MoveEval.Move.SpecialMoveType is not SpecialMoveType.OmnipotentPawnSpawn
        )
    );

    public GamePlayer CreateBotPlayer(GameColor color) =>
        new(
            UserId: BotId,
            Color: color,
            UserName: "Lobotomized Lobotomized Anarchy Bot",
            CountryCode: "FR",
            Rating: int.MinValue
        );

    public Task<ErrorOr<MoveEvaluation>> FindMoveAsync(
        IReadOnlyChessBoard board,
        int lastEval,
        CancellationToken token = default
    ) => _botDecision.DecideMoveAsync(board, lastEval, token);
}
