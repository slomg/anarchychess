using System.ComponentModel;

namespace AnarchyChess.Api.Quests.DTOs;

[DisplayName("MyQuestRanking")]
public record MyQuestRankingDto(
    int TotalQuestPoints,
    int TotalRank,
    int MonthlyQuestPoints,
    int MonthlyRank
);
