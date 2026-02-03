using AnarchyChess.Api.Profile.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.ArchivedGames.Entities;

public class PlayerArchive
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }

    public required GameColor Color { get; set; }
    public required string UserName { get; set; }
    public required double FinalTimeRemaining { get; set; }
    public required int? NewRating { get; set; }
    public required int? RatingChange { get; set; }
    public required string CountryCode { get; set; }
}
