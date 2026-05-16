using AnarchyChess.Api.ArchivedGames.Entities;
using AnarchyChess.Api.Auth.Entities;
using AnarchyChess.Api.Game.Entities;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Preferences.Entities;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Quests.Entities;
using AnarchyChess.Api.Social.Entities;
using AnarchyChess.Api.UserRating.Entities;
using AnarchyChess.Api.Vote.Entities;
using AnarchyChess.Api.Vote.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AuthedUser, IdentityRole<UserId>, UserId>(options)
{
    public required DbSet<RefreshToken> RefreshTokens { get; set; }

    public required DbSet<UserPreferences> UserPreferences { get; set; }

    public required DbSet<BlockedUser> BlockedUsers { get; set; }
    public required DbSet<StarredUser> StarredUsers { get; set; }

    public required DbSet<CurrentRating> CurrentRatings { get; set; }
    public required DbSet<RatingArchive> RatingArchives { get; set; }

    public required DbSet<ChatMessage> MessagesLogs { get; set; }

    public required DbSet<GameArchive> GameArchives { get; set; }
    public required DbSet<PlayerArchive> PlayerArchives { get; set; }

    public required DbSet<UserQuestPoints> QuestPoints { get; set; }

    public required DbSet<UserVote> UserVotes { get; set; }
    public required DbSet<PendingUserVote> PendingUserVotes { get; set; }
    public required DbSet<VoteOptionPair> VoteOptionPairs { get; set; }
    public required DbSet<VoteOption> VoteOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<StarredUser>().Navigation(x => x.Starred).AutoInclude();
        builder.Entity<BlockedUser>().Navigation(x => x.Blocked).AutoInclude();
        builder.Entity<UserQuestPoints>().Navigation(x => x.User).AutoInclude();

        builder.Entity<UserVote>().Navigation(x => x.VotePair).AutoInclude();
        builder.Entity<PendingUserVote>().Navigation(x => x.VotePair).AutoInclude();
        builder.Entity<VoteOptionPair>().Navigation(x => x.OptionA).AutoInclude();
        builder.Entity<VoteOptionPair>().Navigation(x => x.OptionB).AutoInclude();

        base.OnModelCreating(builder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<UserId>()
            .HaveConversion<StructStringValueConverter<UserId>>();
        configurationBuilder
            .Properties<GameToken>()
            .HaveConversion<StructStringValueConverter<GameToken>>();
        configurationBuilder
            .Properties<VoteOptionKey>()
            .HaveConversion<StructStringValueConverter<VoteOptionKey>>();
        base.ConfigureConventions(configurationBuilder);
    }
}
