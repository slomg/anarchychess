import { render, screen } from "@testing-library/react";

import {
    createFakeGuestUser,
    createFakePrivateUser,
} from "@/lib/testUtils/fakers/userFaker";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import DailyQuestRankCard from "../DailyQuestRankCard";
import { GuestUser, PrivateUser } from "@/lib/apiClient";

describe("DailyQuestRankCard", () => {
    let authedUserMock: PrivateUser;
    let guestUserMock: GuestUser;

    beforeEach(() => {
        authedUserMock = createFakePrivateUser();
        guestUserMock = createFakeGuestUser();
    });

    it("should render the card with correct user info and quest points", () => {
        const questPoints = 15;
        const currentRank = 23;
        const totalPlayers = 456;

        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestRankCard
                    rank={{ questPoints, currentRank, totalPlayers }}
                />
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("minimalProfileRowUsername"),
        ).toHaveTextContent(authedUserMock.userName);
        expect(screen.getByTestId("dailyQuestRankPoints")).toHaveTextContent(
            `${questPoints} points`,
        );
        expect(screen.getByTestId("rankDisplayNumber")).toHaveTextContent(
            `#${currentRank}`,
        );

        const expectedPercentile =
            ((totalPlayers - currentRank) / totalPlayers) * 100;
        expect(screen.getByTestId("rankDisplayPercentile")).toHaveTextContent(
            `That's top ${expectedPercentile.toFixed(1)}%!`,
        );
    });

    it("should render guest state correctly", () => {
        render(
            <SessionProvider user={guestUserMock}>
                <DailyQuestRankCard
                    rank={{
                        questPoints: 0,
                        currentRank: 5,
                        totalPlayers: 100,
                    }}
                />
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("minimalProfileRowUsername"),
        ).toHaveTextContent(guestUserMock.userName);

        expect(screen.getByTestId("dailyQuestRankPoints")).toHaveTextContent(
            "0 points",
        );

        expect(
            screen.getByTestId("dailyQuestRankGuestRankNumber"),
        ).toHaveTextContent("-");

        expect(screen.getByText("Guests are unranked")).toBeInTheDocument();

        expect(
            screen.queryByTestId("rankDisplayNumber"),
        ).not.toBeInTheDocument();
    });
});
