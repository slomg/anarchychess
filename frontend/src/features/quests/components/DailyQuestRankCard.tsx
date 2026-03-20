"use client";

import Link from "next/link";

import MinimalProfileView from "@/features/profile/components/MinimalProfileView";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import { isAuthed } from "@/features/auth/lib/userGuard";
import ProgressBar from "@/components/ui/ProgressBar";
import RankDisplay from "@/components/RankDisplay";
import constants from "@/lib/constants";
import Card from "@/components/ui/Card";

export interface QuestRank {
    questPoints: number;
    currentRank: number;
    totalPlayers: number;
}

const DailyQuestRankCard = ({ rank }: { rank: QuestRank }) => {
    const user = useSessionUser();
    if (user === null) {
        return null;
    }

    return (
        <div className="flex w-full flex-row gap-5 overflow-x-auto">
            <Card className="min-w-45 flex-1 justify-center">
                {isAuthed(user) ? (
                    <RankDisplay
                        rank={rank.currentRank}
                        totalPlayers={rank.totalPlayers}
                    />
                ) : (
                    <>
                        <h2 className="text-xl font-bold">Your Rank</h2>
                        <div className="flex items-center gap-3">
                            <p
                                className="text-2xl font-extrabold
                                    text-amber-400"
                                data-testid="dailyQuestRankGuestRankNumber"
                            >
                                -
                            </p>
                            <ProgressBar percent={0} />
                        </div>

                        <Link
                            href={constants.PATHS.SIGNIN}
                            className="text-text/70 text-nowrap"
                        >
                            Guests are unranked
                        </Link>
                    </>
                )}
            </Card>

            <Card>
                <MinimalProfileView
                    profile={user}
                    className="w-max flex-nowrap"
                >
                    <p className="ml-auto" data-testid="dailyQuestRankPoints">
                        {rank.questPoints} points
                    </p>
                </MinimalProfileView>
            </Card>
        </div>
    );
};
export default DailyQuestRankCard;
