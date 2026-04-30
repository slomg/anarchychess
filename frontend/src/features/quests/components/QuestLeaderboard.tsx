"use client";

import {
    getMonthlyQuestLeaderboard,
    getTotalQuestLeaderboard,
    PagedResultOfQuestPointsDto,
} from "@/lib/apiClient";

import LeaderboardMinimalProfileView from "@/features/profile/components/LeaderboardMinimalProfileView";
import PaginatedItemsRenderer from "@/features/pagination/components/PaginatedItemsRenderer";
import QuestLeaderboardResetCountdown from "./QuestLeaderboardResetCountdown";
import { QuestLeaderboardType } from "../lib/types";
import Card from "@/components/ui/Card";

const QuestLeaderboard = ({
    leaderboardType,
    initialLeaderboard,
}: {
    leaderboardType: QuestLeaderboardType;
    initialLeaderboard: PagedResultOfQuestPointsDto;
}) => {
    const fetchItems =
        leaderboardType === QuestLeaderboardType.MONTHLY
            ? getMonthlyQuestLeaderboard
            : getTotalQuestLeaderboard;

    return (
        <Card className="w-full flex-1 gap-5 p-6">
            <div
                className="flex flex-col flex-wrap items-center justify-center
                    gap-2 sm:flex-row sm:justify-between"
            >
                <h1 className="text-2xl" data-testid="questLeaderboardTitle">
                    Quest Leaderboard (
                    {leaderboardType === QuestLeaderboardType.MONTHLY
                        ? "Monthly"
                        : "All Time"}
                    )
                </h1>

                {leaderboardType === QuestLeaderboardType.MONTHLY && (
                    <QuestLeaderboardResetCountdown />
                )}
            </div>

            {initialLeaderboard.totalCount === 0 && (
                <p className="text-error text-center text-2xl">
                    No Players Yet
                </p>
            )}

            <PaginatedItemsRenderer
                fetchItems={fetchItems}
                initialPaged={initialLeaderboard}
            >
                {({ items, page, pageSize }) => (
                    <div className="grid grid-cols-[max-content_1fr] gap-3">
                        {items.map((profileQuestPoints, index) => {
                            const points =
                                leaderboardType === QuestLeaderboardType.MONTHLY
                                    ? profileQuestPoints.monthlyQuestPoints
                                    : profileQuestPoints.totalQuestPoints;
                            return (
                                <LeaderboardMinimalProfileView
                                    profile={profileQuestPoints.profile}
                                    page={page}
                                    pageSize={pageSize}
                                    index={index}
                                    key={profileQuestPoints.profile.userId}
                                >
                                    <p
                                        className="ml-auto flex items-center
                                            gap-2"
                                        data-testid={`questLeaderboardPoints-${profileQuestPoints.profile.userId}`}
                                    >
                                        {points} points
                                    </p>
                                </LeaderboardMinimalProfileView>
                            );
                        })}
                    </div>
                )}
            </PaginatedItemsRenderer>
        </Card>
    );
};
export default QuestLeaderboard;
