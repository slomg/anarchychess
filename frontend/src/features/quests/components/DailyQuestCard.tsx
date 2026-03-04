"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

import {
    collectQuestReward,
    Quest,
    QuestDifficulty,
    replaceDailyQuest,
} from "@/lib/apiClient";

import ProgressBar from "@/components/ui/ProgressBar";
import NewQuestCountdown from "./NewQuestCountdown";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";

const DailyQuestCard = ({ initialQuest }: { initialQuest: Quest }) => {
    const [quest, setQuest] = useState(initialQuest);
    const [error, setError] = useState("");
    const [isFetching, setIsFetching] = useState(false);
    const router = useRouter();

    const percentDone = (quest.progress / quest.target) * 100;
    const difficultyText =
        QuestDifficulty[quest.difficulty].charAt(0).toUpperCase() +
        QuestDifficulty[quest.difficulty].slice(1).toLowerCase();
    const isCompleted = quest.progress >= quest.target;

    const difficultyColor = {
        [QuestDifficulty.EASY]: "text-green-400",
        [QuestDifficulty.MEDIUM]: "text-yellow-400",
        [QuestDifficulty.HARD]: "text-red-400",
    };

    async function onReplaceQuest() {
        setIsFetching(true);

        try {
            const { error, data: newQuest } = await replaceDailyQuest();
            if (error || !newQuest) {
                setError("Failed to replace quest");
                console.error(
                    "DailyQuestCard onReplaceQuest replaceDailyQuest",
                    error,
                );
                return;
            }

            setQuest(newQuest);
            router.refresh();
        } finally {
            setIsFetching(false);
        }
    }

    async function onCollectReward() {
        setIsFetching(true);
        try {
            const { error } = await collectQuestReward();
            if (error) {
                setError("Failed to collect reward");
                console.error(
                    "DailyQuestCard onCollectReward collectQuestReward",
                    error,
                );
                return;
            }

            setQuest({
                ...quest,
                streak: quest.streak + 1,
                rewardCollected: true,
            });
            router.refresh();
        } finally {
            setIsFetching(false);
        }
    }

    const renderActionButton = () => {
        if (!isCompleted && quest.canReplace)
            return (
                <Button
                    data-testid="dailyQuestReplaceButton"
                    onClick={onReplaceQuest}
                    className="py-1"
                    disabled={isFetching}
                >
                    Replace
                </Button>
            );

        if (isCompleted && !quest.rewardCollected)
            return (
                <Button
                    data-testid="dailyQuestCollectButton"
                    onClick={onCollectReward}
                    className="py-1"
                    disabled={isFetching}
                >
                    Collect Reward
                </Button>
            );

        if (isCompleted && quest.rewardCollected)
            return (
                <p
                    data-testid="dailyQuestCollectedRewardText"
                    className={difficultyColor[quest.difficulty]}
                >
                    +{quest.difficulty} points
                </p>
            );

        return null;
    };

    return (
        <Card className="p-6">
            {/* quest */}
            <div className="flex flex-col gap-2">
                <p
                    className="text-lg sm:text-start"
                    data-testid="dailyQuestDescription"
                >
                    <span
                        className={difficultyColor[quest.difficulty]}
                        data-testid="dailyQuestDifficulty"
                    >
                        {difficultyText}:
                    </span>{" "}
                    {quest.description}
                </p>

                <div className="flex items-center">
                    <ProgressBar percent={percentDone} />

                    <p
                        className="text-text/70 min-w-10 text-center text-sm
                            font-medium"
                        data-testid="dailyQuestProgressText"
                    >
                        {quest.progress}/{quest.target}
                    </p>

                    {renderActionButton()}
                </div>

                {error && (
                    <p className="text-error" data-testid="dailyQueryError">
                        {error}
                    </p>
                )}

                {/* footer */}
                <div
                    className="text-text/70 flex flex-wrap justify-center
                        gap-x-3 text-center sm:justify-between"
                >
                    <NewQuestCountdown />

                    <span data-testid="dailyQuestStreak">
                        {quest.streak > 0 && "🔥"}
                        {quest.streak} Day{quest.streak === 1 ? "" : "s"} Streak
                    </span>
                </div>
            </div>
        </Card>
    );
};
export default DailyQuestCard;
