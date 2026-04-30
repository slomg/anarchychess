import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import {
    getMonthlyQuestLeaderboard,
    getTotalQuestLeaderboard,
} from "@/lib/apiClient";

import { createFakePagedUserQuestPoints } from "@/lib/testUtils/fakers/userQuestPointsFaker";
import { QuestLeaderboardType } from "../../lib/types";
import QuestLeaderboard from "../QuestLeaderboard";

vi.mock("@/lib/apiClient/definition");

describe("QuestLeaderboard", () => {
    const getMonthlyQuestLeaderboardMock = vi.mocked(
        getMonthlyQuestLeaderboard,
    );
    const getTotalQuestLeaderboardMock = vi.mocked(getTotalQuestLeaderboard);

    it("should render heading and countdown for monthly leaderboard", () => {
        const initialLeaderboard = createFakePagedUserQuestPoints();
        render(
            <QuestLeaderboard
                initialLeaderboard={initialLeaderboard}
                leaderboardType={QuestLeaderboardType.MONTHLY}
            />,
        );

        expect(screen.getByRole("heading")).toHaveTextContent(
            "Quest Leaderboard (Monthly)",
        );
        expect(
            screen.getByTestId("leaderboardResetCountdown"),
        ).toBeInTheDocument();
    });

    it("should render heading for all time leaderboard", () => {
        const initialLeaderboard = createFakePagedUserQuestPoints();
        render(
            <QuestLeaderboard
                initialLeaderboard={initialLeaderboard}
                leaderboardType={QuestLeaderboardType.ALL_TIME}
            />,
        );

        expect(screen.getByRole("heading")).toHaveTextContent(
            "Quest Leaderboard (All Time)",
        );
        expect(
            screen.queryByTestId("leaderboardResetCountdown"),
        ).not.toBeInTheDocument();
    });

    it.each([
        [QuestLeaderboardType.MONTHLY, "monthlyQuestPoints" as const],
        [QuestLeaderboardType.ALL_TIME, "totalQuestPoints" as const],
    ])(
        "should render correct points for %s leaderboard",
        (leaderboardType, pointsField) => {
            const initialLeaderboard = createFakePagedUserQuestPoints();
            const firstItem = initialLeaderboard.items[0];
            render(
                <QuestLeaderboard
                    initialLeaderboard={initialLeaderboard}
                    leaderboardType={leaderboardType}
                />,
            );

            expect(
                screen.getByTestId(
                    `leaderboardItem-${firstItem.profile.userId}`,
                ),
            ).toBeInTheDocument();
            expect(
                screen.getByTestId(
                    `questLeaderboardPoints-${firstItem.profile.userId}`,
                ),
            ).toHaveTextContent(`${firstItem[pointsField]} points`);
        },
    );

    it.each([
        [QuestLeaderboardType.MONTHLY, getMonthlyQuestLeaderboardMock],
        [QuestLeaderboardType.ALL_TIME, getTotalQuestLeaderboardMock],
    ])(
        "should fetch the next page when pagination is triggered",
        async (leaderboardType, fetchItems) => {
            const user = userEvent.setup();
            const pageSize = 1;

            const firstPage = createFakePagedUserQuestPoints({
                pagination: { pageSize, totalCount: 2, page: 0 },
            });
            const secondPage = createFakePagedUserQuestPoints({
                pagination: { pageSize, totalCount: 2, page: 1 },
            });

            fetchItems.mockResolvedValueOnce({
                data: secondPage,
                response: new Response(),
            });

            render(
                <QuestLeaderboard
                    leaderboardType={leaderboardType}
                    initialLeaderboard={firstPage}
                />,
            );

            const nextBtn = screen.getByTestId("paginationNext");
            await user.click(nextBtn);

            expect(fetchItems).toHaveBeenCalledWith({
                query: {
                    Page: 1,
                    PageSize: pageSize,
                },
            });

            const newItem = secondPage.items[0];
            expect(
                await screen.findByTestId(
                    `leaderboardItem-${newItem.profile.userId}`,
                ),
            ).toBeInTheDocument();
        },
    );

    it("should display no players text when there are no leaderboard spots", () => {
        const firstPage = createFakePagedUserQuestPoints({
            pagination: { totalCount: 0 },
        });

        render(
            <QuestLeaderboard
                leaderboardType={QuestLeaderboardType.MONTHLY}
                initialLeaderboard={firstPage}
            />,
        );

        expect(screen.getByText("No Players Yet")).toBeInTheDocument();
    });
});
