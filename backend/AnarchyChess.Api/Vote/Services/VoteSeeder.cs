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
            Name = "a",
            Description = "1",
        },
        new()
        {
            Key = "b",
            Name = "b",
            Description = "2",
        },
        new()
        {
            Key = "c",
            Name = "c",
            Description = "3",
        },
        new()
        {
            Key = "d",
            Name = "d",
            Description = "4",
        },
        new()
        {
            Key = "e",
            Name = "e",
            Description = "5",
        },
        new()
        {
            Key = "f",
            Name = "f",
            Description = "6",
        },
        new()
        {
            Key = "g",
            Name = "g",
            Description = "7",
        },
        new()
        {
            Key = "h",
            Name = "h",
            Description = "8",
        },
        new()
        {
            Key = "i",
            Name = "i",
            Description = "9",
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
