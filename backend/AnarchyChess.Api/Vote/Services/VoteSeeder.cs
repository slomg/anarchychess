using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.Vote.Entities;
using AnarchyChess.Api.Vote.Repositories;

namespace AnarchyChess.Api.Vote.Services;

public interface IVoteSeeder
{
    Task SeedAsync(CancellationToken token = default);
}

public class VoteSeeder(IVoteRepository voteRepository, IUnitOfWork unitOfWork) : IVoteSeeder
{
    private readonly List<VoteOption> _options =
    [
        new()
        {
            Key = "a",
            Name = "Pawn Bankruptcy",
            Description =
                "If a pawn reaches the last rank and is not promoted immediately, it is removed from the board for indecision.",
        },
        new()
        {
            Key = "b",
            Name = "Knight Momentum",
            Description =
                "Knights may continue moving in the same direction if they capture a piece, chaining knight moves in a straight flow.",
        },
        new()
        {
            Key = "c",
            Name = "Royal Confusion",
            Description =
                "If a king is attacked twice in a row from different pieces, it must randomly teleport to any empty square.",
        },
        new()
        {
            Key = "d",
            Name = "Bishop Diagonal Law",
            Description =
                "Bishops may permanently convert one diagonal they travel on into blocked terrain for both players.",
        },
        new()
        {
            Key = "e",
            Name = "Rook Lockdown",
            Description =
                "When a rook captures a piece, the captured piece’s entire row becomes frozen for one turn.",
        },
        new()
        {
            Key = "f",
            Name = "En Passant Echo",
            Description =
                "Every en passant capture creates a phantom pawn that repeats the same move on the next turn before disappearing.",
        },
        new()
        {
            Key = "g",
            Name = "Queen Fracture",
            Description =
                "A queen may split into a rook and bishop once per game, both becoming independent pieces permanently.",
        },
        new()
        {
            Key = "h",
            Name = "Check Instability",
            Description =
                "Delivering check forces the checking piece to move again immediately if it has a legal move.",
        },
        new()
        {
            Key = "i",
            Name = "Board Rotation Shift",
            Description =
                "After any capture, the board rotates 90 degrees clockwise, preserving piece positions relative to the grid.",
        },
    ];

    private readonly IVoteRepository _voteRepository = voteRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task SeedAsync(CancellationToken token = default)
    {
        await _voteRepository.BulkAddVoteOptionsIfNotExistAsync(_options, token);

        List<VoteOptionPair> pairs = [];
        for (int i = 0; i < _options.Count; i++)
        {
            var optionA = _options[i];
            for (int j = i + 1; j < _options.Count; j++)
            {
                var optionB = _options[j];
                pairs.Add(
                    new VoteOptionPair()
                    {
                        OptionAKey = optionA.Key,
                        OptionA = optionA,
                        OptionBKey = optionB.Key,
                        OptionB = optionB,
                    }
                );
            }
        }

        await _voteRepository.BulkAddVoteOptionPairsIfNotExistAsync(pairs, token);
        await _unitOfWork.CompleteAsync(token);
    }
}
