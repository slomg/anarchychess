import { Metadata } from "next";

import {
    getDailyQuest,
    getMyQuestRanking,
    getQuestLeaderboard,
    getUserQuestPoints,
} from "@/lib/apiClient";

import DailyQuestCardLoggedOut from "@/features/quests/components/DailyQuestCardLoggedOut";
import DailyQuestRankCard from "@/features/quests/components/DailyQuestRankCard";
import WithOptionalAuthedUser from "@/features/auth/hocs/WithOptionalAuthedUser";
import QuestLeaderboard from "@/features/quests/components/QuestLeaderboard";
import DailyQuestTitle from "@/features/quests/components/DailyQuestTitle";
import DailyQuestCard from "@/features/quests/components/DailyQuestCard";
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
        <WithOptionalAuthedUser>
            {async ({ accessToken, user }) => {
                const [
                    leaderboard,
                    dailyQuest,
                    userCurrentRank,
                    userQuestPoints,
                ] = await Promise.all([
                    dataOrThrow(
                        getQuestLeaderboard({
                            query: {
                                Page: 0,
                                PageSize:
                                    constants.PAGINATION_PAGE_SIZE
                                        .QUEST_LEADERBOARD,
                            },
                        }),
                    ),
                    (async () => {
                        if (!accessToken) return;

                        return await dataOrThrow(
                            getDailyQuest({
                                auth: () => accessToken,
                            }),
                        );
                    })(),
                    (async () => {
                        if (!accessToken) return;

                        return await dataOrThrow(
                            getMyQuestRanking({ auth: () => accessToken }),
                        );
                    })(),
                    (async () => {
                        if (!user) return;

                        return await dataOrThrow(
                            getUserQuestPoints({
                                path: { userId: user.userId },
                            }),
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

                            {dailyQuest ? (
                                <DailyQuestCard initialQuest={dailyQuest} />
                            ) : (
                                <DailyQuestCardLoggedOut />
                            )}
                        </div>

                        <DailyQuestRankCard
                            questPoints={userQuestPoints}
                            currentRank={userCurrentRank}
                            totalPlayers={leaderboard.totalCount}
                        />

                        <QuestLeaderboard initialLeaderboard={leaderboard} />
                    </main>
                );
            }}
        </WithOptionalAuthedUser>
    );
}
