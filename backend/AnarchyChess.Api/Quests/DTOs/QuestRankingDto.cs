using System.ComponentModel;

namespace AnarchyChess.Api.Quests.DTOs;

[DisplayName("QuestRanking")]
public record QuestRankingDto(
    int TotalQuestPoints,
    int TotalRank,
    int MonthlyQuestPoints,
    int MonthlyRank
);
