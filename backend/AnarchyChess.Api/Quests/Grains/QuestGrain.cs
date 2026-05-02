using AnarchyChess.Api.Bots.Grains;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.QuestLogic;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.Quests.DTOs;
using AnarchyChess.Api.Quests.Errors;
using AnarchyChess.Api.Quests.Services;
using AnarchyChess.Api.Streaming;
using ErrorOr;
using Orleans.Streams;

namespace AnarchyChess.Api.Quests.Grains;

[Alias("AnarchyChess.Api.Quests.Grains.IQuestGrain")]
public interface IQuestGrain : IGrainWithStringKey
{
    [Alias("CollectRewardAsync")]
    Task<ErrorOr<int>> CollectRewardAsync(CancellationToken token = default);

    [Alias("GetGuestAsync")]
    Task<QuestDto> GetQuestAsync(CancellationToken token = default);

    [Alias("ReplaceQuestAsync")]
    Task<ErrorOr<QuestDto>> ReplaceQuestAsync(CancellationToken token = default);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Quests.Grains.QuestGrainStorage")]
public class QuestGrainStorage
{
    [Id(0)]
    public QuestInstance? Quest { get; set; }

    [Id(3)]
    public bool CanReplace { get; set; } = true;

    [Id(4)]
    public bool RewardCollected { get; set; }

    [Id(5)]
    public int Streak { get; set; }

    public void CompleteQuest()
    {
        if (Quest is null)
            return;

        CanReplace = false;
    }

    public void SelectNewQuest(QuestInstance quest)
    {
        // reset streak if last quest was missed, failed, not claimed, or is too old
        var isSameDay = quest.CreationDate.DayNumber == Quest?.CreationDate.DayNumber;
        var isTooOld = quest.CreationDate.DayNumber - Quest?.CreationDate.DayNumber > 1;
        var previousQuestIncomplete =
            Quest is null || !Quest.IsCompleted || !RewardCollected || isTooOld;
        if (!isSameDay && previousQuestIncomplete)
        {
            Streak = 0;
        }

        Quest = quest;
        CanReplace = true;
        RewardCollected = false;
    }

    public void MarkRewardCollected()
    {
        RewardCollected = true;
        Streak++;
    }
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Quests.Grains.QuestEndStreamState")]
public class QuestEndStreamState : StreamState;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Quests.Grains.QuestBotEndStreamState")]
public class QuestBotEndStreamState : StreamState;

[ImplicitStreamSubscription(nameof(GameEndedEvent))]
public class QuestGrain(
    ILogger<QuestGrain> logger,
    [PersistentState(QuestGrain.StateName)] IPersistentState<QuestGrainStorage> state,
    [PersistentState(QuestGrain.StateName + "GameEndStream")]
        IPersistentState<QuestEndStreamState> gameEndStreamState,
    [PersistentState(QuestGrain.StateName + "BotGameEndStream")]
        IPersistentState<QuestBotEndStreamState> botGameEndStreamState,
    IQuestService questService,
    IRandomQuestProvider questProvider,
    TimeProvider timeProvider
) : Grain, IQuestGrain, IAsyncObserver<GameEndedEvent>, IAsyncObserver<BotGameEndedEvent>
{
    public const string StateName = "quest";

    private readonly ILogger<QuestGrain> _logger = logger;
    private readonly IPersistentState<QuestGrainStorage> _state = state;
    private readonly IPersistentState<QuestEndStreamState> _gameEndStreamState = gameEndStreamState;
    private readonly IPersistentState<QuestBotEndStreamState> _botGameEndStreamState =
        botGameEndStreamState;
    private readonly IQuestService _questService = questService;
    private readonly IRandomQuestProvider _questProvider = questProvider;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<QuestDto> GetQuestAsync(CancellationToken token = default)
    {
        var quest = GetOrSelectQuest();
        await _state.WriteStateAsync(token);
        return ToDto(quest);
    }

    public async Task<ErrorOr<QuestDto>> ReplaceQuestAsync(CancellationToken token = default)
    {
        if (!_state.State.CanReplace)
        {
            return QuestErrors.CanotReplace;
        }

        var quest = SelectNewQuest();
        _state.State.CanReplace = false;
        await _state.WriteStateAsync(token);

        return ToDto(quest);
    }

    public async Task<ErrorOr<int>> CollectRewardAsync(CancellationToken token = default)
    {
        if (_state.State.RewardCollected)
        {
            return QuestErrors.NoRewardToCollect;
        }

        var quest = GetOrSelectQuest();
        if (!quest.IsCompleted)
        {
            return QuestErrors.NoRewardToCollect;
        }

        var userId = this.GetPrimaryKeyString();
        await _questService.IncrementQuestPointsAsync(userId, (int)quest.Difficulty, token);

        _state.State.MarkRewardCollected();
        await _state.WriteStateAsync(token);

        return (int)quest.Difficulty;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);

        var streamProvider = this.GetStreamProvider(StreamingConstants.StreamProvider);
        var gameEndStream = streamProvider.GetStream<GameEndedEvent>(
            nameof(GameEndedEvent),
            this.GetPrimaryKeyString()
        );
        await gameEndStream.SubscribeAsync(this, _gameEndStreamState.State.SequenceToken);

        var botGameEndStream = streamProvider.GetStream<BotGameEndedEvent>(
            nameof(BotGameEndedEvent),
            this.GetPrimaryKeyString()
        );
        await botGameEndStream.SubscribeAsync(this, _botGameEndStreamState.State.SequenceToken);
    }

    public async Task OnNextAsync(GameEndedEvent @event, StreamSequenceToken? token = null)
    {
        if (!_gameEndStreamState.State.TryUpdateSequenceToken(token))
        {
            return;
        }
        await _gameEndStreamState.WriteStateAsync();

        if (@event.EndStatus.Result is GameResult.Aborted)
        {
            return;
        }

        var grain = GrainFactory.GetGrain<IGameGrain>(@event.GameToken);
        var stateResult = await grain.GetStateAsync();
        if (stateResult.IsError)
        {
            _logger.LogWarning(
                "Could not find state for quest on bot game {GameToken}, {Errors}",
                @event.GameToken,
                stateResult.Errors
            );
            return;
        }
        var state = stateResult.Value;

        var snapshot = BuildQuestSnapshot(
            @event.GameToken,
            whitePlayer: state.WhitePlayer,
            blackPlayer: state.BlackPlayer,
            boardResult: await grain.GetBoardAsync(),
            resultData: @event.EndStatus,
            pool: state.Pool,
            clocks: state.Clocks
        );
        await ReceiveQuestSnapshotAsync(snapshot);
    }

    public async Task OnNextAsync(BotGameEndedEvent @event, StreamSequenceToken? token = null)
    {
        if (!_botGameEndStreamState.State.TryUpdateSequenceToken(token))
        {
            return;
        }
        await _botGameEndStreamState.WriteStateAsync();

        var grain = GrainFactory.GetGrain<IBotGrain>(@event.GameToken);
        var stateResult = await grain.GetStateAsync();
        if (stateResult.IsError)
        {
            _logger.LogWarning(
                "Could not find state for quest on bot game {GameToken}, {Errors}",
                @event.GameToken,
                stateResult.Errors
            );
            return;
        }
        var state = stateResult.Value;

        var snapshot = BuildQuestSnapshot(
            @event.GameToken,
            whitePlayer: state.WhitePlayer,
            blackPlayer: state.BlackPlayer,
            boardResult: await grain.GetBoardAsync(),
            resultData: @event.EndStatus,
            pool: null,
            clocks: null
        );
        await ReceiveQuestSnapshotAsync(snapshot);
    }

    public Task OnErrorAsync(Exception ex)
    {
        _logger.LogError(ex, "Error in quest grain game stream");
        return Task.CompletedTask;
    }

    private QuestInstance GetOrSelectQuest()
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        if (_state.State.Quest is not null && _state.State.Quest.CreationDate == today)
        {
            return _state.State.Quest;
        }

        return SelectNewQuest();
    }

    private QuestInstance SelectNewQuest()
    {
        var quest = _questProvider.GetRandomQuestInstance(except: _state.State.Quest);
        _state.State.SelectNewQuest(quest);

        return quest;
    }

    private QuestDto ToDto(QuestInstance quest) =>
        new(
            Difficulty: quest.Difficulty,
            Description: quest.Description,
            Target: quest.Target,
            Progress: quest.Progress,
            CanReplace: _state.State.CanReplace,
            RewardCollected: _state.State.RewardCollected,
            Streak: _state.State.Streak
        );

    private async Task ReceiveQuestSnapshotAsync(GameQuestSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return;
        }

        var quest = GetOrSelectQuest();
        if (quest.IsCompleted)
        {
            return;
        }

        quest.ApplySnapshot(snapshot);
        if (quest.IsCompleted)
        {
            _state.State.CompleteQuest();
        }

        await _state.WriteStateAsync();
    }

    private GameQuestSnapshot? BuildQuestSnapshot(
        GameToken gameToken,
        GamePlayer whitePlayer,
        GamePlayer blackPlayer,
        ErrorOr<IReadOnlyChessBoard> boardResult,
        GameResultData resultData,
        PoolKey? pool,
        ClockSnapshot? clocks
    )
    {
        UserId userId = this.GetPrimaryKeyString();
        if (userId != whitePlayer.UserId && userId != blackPlayer.UserId)
        {
            _logger.LogWarning(
                "Could not find player {UserId} for quest on game {GameToken}",
                this.GetPrimaryKeyString(),
                gameToken
            );
            return null;
        }
        GamePlayer player = userId == whitePlayer.UserId ? whitePlayer : blackPlayer;

        if (boardResult.IsError)
        {
            _logger.LogWarning(
                "Could not find board for quest on game {GameToken}, {Errors}",
                gameToken,
                boardResult.Errors
            );
            return null;
        }

        return new(
            PlayerColor: player.Color,
            Board: boardResult.Value,
            ResultData: resultData,
            Pool: pool,
            Clocks: clocks
        );
    }
}
