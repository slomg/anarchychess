import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { createFakePagedUserQuestPoints } from "@/lib/testUtils/fakers/userQuestPointsFaker";
import { createFakeQuestRanking } from "@/lib/testUtils/fakers/questRankingFaker";
import QuestLeaderboardSelection from "../QuestLeaderboardSelection";
import { QuestLeaderboardType } from "../../lib/types";
import DailyQuestRankCard from "../DailyQuestRankCard";
import QuestLeaderboard from "../QuestLeaderboard";

vi.mock("../DailyQuestRankCard");
vi.mock("../QuestLeaderboard");

describe("QuestLeaderboardSelection", () => {
    const monthlyLeaderboard = createFakePagedUserQuestPoints();
    const totalLeaderboard = createFakePagedUserQuestPoints();
    const questRanking = createFakeQuestRanking();

    it("should pass monthly props by default", () => {
        render(
            <QuestLeaderboardSelection
                monthlyLeaderboard={monthlyLeaderboard}
                totalLeaderboard={totalLeaderboard}
                myQuestRanking={questRanking}
            />,
        );

        expect(DailyQuestRankCard).toHaveBeenCalledWith(
            expect.objectContaining({
                questLeaderboardType: QuestLeaderboardType.MONTHLY,
                rank: {
                    questPoints: questRanking.monthlyQuestPoints,
                    currentRank: questRanking.monthlyRank,
                    totalPlayers: monthlyLeaderboard.totalCount,
                },
            }),
            undefined,
        );
        expect(QuestLeaderboard).toHaveBeenCalledWith(
            expect.objectContaining({
                leaderboardType: QuestLeaderboardType.MONTHLY,
                initialLeaderboard: monthlyLeaderboard,
            }),
            undefined,
        );
    });

    it("should pass all time props when all time is selected", async () => {
        const user = userEvent.setup();
        render(
            <QuestLeaderboardSelection
                monthlyLeaderboard={monthlyLeaderboard}
                totalLeaderboard={totalLeaderboard}
                myQuestRanking={questRanking}
            />,
        );

        await user.click(
            screen.getByTestId(`selector-${QuestLeaderboardType.ALL_TIME}`),
        );

        expect(DailyQuestRankCard).toHaveBeenCalledWith(
            expect.objectContaining({
                questLeaderboardType: QuestLeaderboardType.ALL_TIME,
                rank: {
                    questPoints: questRanking.totalQuestPoints,
                    currentRank: questRanking.totalRank,
                    totalPlayers: totalLeaderboard.totalCount,
                },
            }),
            undefined,
        );
        expect(QuestLeaderboard).toHaveBeenCalledWith(
            expect.objectContaining({
                leaderboardType: QuestLeaderboardType.ALL_TIME,
                initialLeaderboard: totalLeaderboard,
            }),
            undefined,
        );
    });
});
