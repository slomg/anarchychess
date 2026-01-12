"use client";

import RankDisplay from "@/components/RankDisplay";
import Card from "@/components/ui/Card";
import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";
import MinimalProfileView from "@/features/profile/components/MinimalProfileView";

const DailyQuestRankCard = ({
    questPoints,
    currentRank,
    totalPlayers,
}: {
    questPoints?: number;
    currentRank?: number;
    totalPlayers: number;
}) => {
    const user = useAuthedUser();
    if (user === null || !currentRank) return null;

    return (
        <div className="flex w-full flex-row gap-5 overflow-x-auto">
            <Card className="flex-1 justify-center">
                <RankDisplay rank={currentRank} totalPlayers={totalPlayers} />
            </Card>

            <Card>
                <MinimalProfileView profile={user}>
                    <p className="ml-auto" data-testid="dailyQuestRankPoints">
                        {questPoints} points
                    </p>
                </MinimalProfileView>
            </Card>
        </div>
    );
};
export default DailyQuestRankCard;
