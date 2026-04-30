import SessionProvider from "@/features/auth/contexts/sessionContext";
import QuestLeaderboardSelection from "../QuestLeaderboardSelection";
import { createFakePagedUserQuestPoints } from "@/lib/testUtils/fakers/userQuestPointsFaker";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QuestLeaderboardType } from "../../lib/types";

describe("QuestLeaderboardSelection", () => {
    const monthlyLeaderboard = createFakePagedUserQuestPoints();
    const totalLeaderboard = createFakePagedUserQuestPoints();

    it("should show monthly leaderboard and rank card by default", () => {
        render(
            <SessionProvider user={createFakePrivateUser()}>
                <QuestLeaderboardSelection
                    monthlyLeaderboard={monthlyLeaderboard}
                    totalLeaderboard={totalLeaderboard}
                />
            </SessionProvider>,
        );

        expect(screen.getByTestId("questLeaderboardTitle")).toHaveTextContent(
            "Quest Leaderboard (Monthly)",
        );
        expect(screen.getByTestId("rankDisplayTitle")).toHaveTextContent(
            "Your Rank (Monthly)",
        );
    });

    it("should show all time leaderboard and rank card when all time is selected", async () => {
        const user = userEvent.setup();
        render(
            <SessionProvider user={createFakePrivateUser()}>
                <QuestLeaderboardSelection
                    monthlyLeaderboard={monthlyLeaderboard}
                    totalLeaderboard={totalLeaderboard}
                />
            </SessionProvider>,
        );

        await user.click(
            screen.getByTestId(`selector-${QuestLeaderboardType.ALL_TIME}`),
        );

        expect(screen.getByTestId("questLeaderboardTitle")).toHaveTextContent(
            "Leaderboard (All Time)",
        );
        expect(screen.getByTestId("rankDisplayTitle")).toHaveTextContent(
            "Your Rank (All Time)",
        );
    });
});
