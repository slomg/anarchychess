"use client";

import { MyQuestRanking, PagedResultOfQuestPointsDto } from "@/lib/apiClient";
import DailyQuestRankCard from "./DailyQuestRankCard";
import { QuestLeaderboardType } from "../lib/types";
import QuestLeaderboard from "./QuestLeaderboard";
import Selector from "@/components/ui/Selector";
import useLocalPref from "@/hooks/useLocalPref";
import constants from "@/lib/constants";

const QuestLeaderboardSelection = ({
    monthlyLeaderboard,
    totalLeaderboard,
    myQuestRanking,
}: {
    monthlyLeaderboard: PagedResultOfQuestPointsDto;
    totalLeaderboard: PagedResultOfQuestPointsDto;
    myQuestRanking?: MyQuestRanking;
}) => {
    const [selectedLeaderboard, setSelectedLeaderboard] = useLocalPref(
        constants.LOCALSTORAGE.PREFERS_QUEST_LEADERBOARD,
        QuestLeaderboardType.MONTHLY,
    );

    return (
        <>
            <Selector
                className="bg-card"
                options={[
                    {
                        label: "Monthly",
                        value: QuestLeaderboardType.MONTHLY,
                    },
                    { label: "All Time", value: QuestLeaderboardType.ALL_TIME },
                ]}
                value={selectedLeaderboard}
                onChange={(e) => setSelectedLeaderboard(e.target.value)}
            />

            {selectedLeaderboard === QuestLeaderboardType.MONTHLY ? (
                <DailyQuestRankCard
                    questLeaderboardType={QuestLeaderboardType.MONTHLY}
                    rank={{
                        questPoints: myQuestRanking?.monthlyQuestPoints ?? 0,
                        currentRank:
                            myQuestRanking?.monthlyRank ??
                            monthlyLeaderboard.totalCount,
                        totalPlayers: monthlyLeaderboard.totalCount,
                    }}
                />
            ) : (
                <DailyQuestRankCard
                    questLeaderboardType={QuestLeaderboardType.ALL_TIME}
                    rank={{
                        questPoints: myQuestRanking?.totalQuestPoints ?? 0,
                        currentRank:
                            myQuestRanking?.totalQuestPoints ??
                            totalLeaderboard.totalCount,
                        totalPlayers: totalLeaderboard.totalCount,
                    }}
                />
            )}

            {selectedLeaderboard === QuestLeaderboardType.MONTHLY ? (
                <QuestLeaderboard
                    leaderboardType={QuestLeaderboardType.MONTHLY}
                    initialLeaderboard={monthlyLeaderboard}
                />
            ) : (
                <QuestLeaderboard
                    leaderboardType={QuestLeaderboardType.ALL_TIME}
                    initialLeaderboard={totalLeaderboard}
                />
            )}
        </>
    );
};
export default QuestLeaderboardSelection;
