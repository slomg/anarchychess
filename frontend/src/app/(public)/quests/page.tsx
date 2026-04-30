import { Metadata } from "next";

import {
    getDailyQuest,
    getMonthlyQuestLeaderboard,
    getMyQuestRanking,
} from "@/lib/apiClient";

import DailyQuestRankCard from "@/features/quests/components/DailyQuestRankCard";
import QuestLeaderboard from "@/features/quests/components/QuestLeaderboard";
import DailyQuestTitle from "@/features/quests/components/DailyQuestTitle";
import DailyQuestCard from "@/features/quests/components/DailyQuestCard";
import WithSession from "@/features/auth/hocs/WithSession";
import { isGuest } from "@/features/auth/lib/userGuard";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";
import constants from "@/lib/constants";

export const metadata: Metadata = {
    title: "Quests - Anarchy Chess",
    description:
        "Complete daily quests, earn points, and climb the leaderboard in Anarchy Chess. Check your rank and compete with other players!",
    keywords: [
        "anarchy chess",
        "chess quests",
        "daily challenges",
        "chess leaderboard",
        "online chess",
    ],
};

export default async function QuestsPage() {
    return (
        <WithSession>
            {async ({ accessToken, user }) => {
                const [
                    monthlyLeaderboard,
                    dailyQuest,
                    userCurrentRank,
                    userQuestPoints,
                ] = await Promise.all([
                    dataOrThrow(
                        getMonthlyQuestLeaderboard({
                            query: {
                                Page: 0,
                                PageSize:
                                    constants.PAGINATION_PAGE_SIZE
                                        .QUEST_LEADERBOARD,
                            },
                        }),
                    ),
                    (async () => {
                        return await dataOrThrow(
                            getDailyQuest({
                                auth: () => accessToken,
                            }),
                        );
                    })(),
                    (async () => {
                        if (isGuest(user)) {
                            return;
                        }

                        return await dataOrThrow(
                            getMyQuestRanking({ auth: () => accessToken }),
                        );
                    })(),
                    (async () => {
                        if (isGuest(user)) {
                            return;
                        }

                        return await dataOrThrow(
                            getMyQuestRanking({ auth: () => accessToken }),
                        );
                    })(),
                ]);

                return (
                    <main
                        className="mx-auto flex w-full max-w-7xl min-w-0 flex-1
                            flex-col gap-5 p-6"
                    >
                        <div
                            className="grid grid-cols-1 gap-5
                                md:grid-cols-[auto_1fr]"
                        >
                            <DailyQuestTitle />
                            <DailyQuestCard initialQuest={dailyQuest} />
                        </div>

                        <DailyQuestRankCard
                            rank={{
                                questPoints: userQuestPoints ?? 0,
                                currentRank:
                                    userCurrentRank ??
                                    monthlyLeaderboard.totalCount,
                                totalPlayers: monthlyLeaderboard.totalCount,
                            }}
                        />

                        <QuestLeaderboard
                            initialLeaderboard={monthlyLeaderboard}
                        />
                    </main>
                );
            }}
        </WithSession>
    );
}
