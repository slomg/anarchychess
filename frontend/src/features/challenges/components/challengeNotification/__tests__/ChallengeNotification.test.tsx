import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import {
    acceptChallenge,
    cancelChallenge,
    ChallengeRequest,
    PoolType,
} from "@/lib/apiClient";

import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import gameStartRedirect from "@/features/liveGame/lib/gameStartRedirect";
import ChallengeNotification from "../ChallengeNotification";

vi.mock("@/lib/apiClient/definition");
vi.mock("@/features/liveGame/lib/gameStartRedirect");

describe("ChallengeNotification", () => {
    const cancelChallengeMock = vi.mocked(cancelChallenge);
    const acceptChallengeMock = vi.mocked(acceptChallenge);
    const gameToken = "test game";
    const removeChallenge = vi.fn();

    let challenge: ChallengeRequest;

    beforeEach(() => {
        cancelChallengeMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
        acceptChallengeMock.mockResolvedValue({
            data: gameToken,
            response: new Response(),
        });

        challenge = createFakeChallengeRequest();
    });

    it("should render challenge info", () => {
        render(
            <ChallengeNotification
                challenge={challenge}
                removeChallenge={removeChallenge}
            />,
        );

        expect(
            screen.getByTestId(
                `challengeNotification-${challenge.challengeToken}`,
            ),
        ).toBeInTheDocument();
        expect(
            screen.getByTestId("challengeNotificationUsername"),
        ).toHaveTextContent(challenge.requester.userName);
        expect(
            screen.getByTestId("challengeNotificationTimeControl"),
        ).toHaveTextContent(
            `${challenge.pool.timeControl.baseSeconds / 60}+${challenge.pool.timeControl.incrementSeconds}`,
        );
    });

    it.each([
        [PoolType.RATED, "rated"],
        [PoolType.CASUAL, "casual"],
    ])("should render the correct pool type", (pool, text) => {
        challenge.pool.poolType = pool;
        render(
            <ChallengeNotification
                challenge={challenge}
                removeChallenge={removeChallenge}
            />,
        );

        expect(
            screen.getByTestId("challengeNotificationPoolType"),
        ).toHaveTextContent(text);
    });

    it("should call cancelChallenge when decline button clicked", async () => {
        const user = userEvent.setup();
        render(
            <ChallengeNotification
                challenge={challenge}
                removeChallenge={removeChallenge}
            />,
        );

        await user.click(screen.getByTestId("challengeNotificationDecline"));

        expect(cancelChallenge).toHaveBeenCalledWith({
            path: { challengeToken: challenge.challengeToken },
        });

        expect(removeChallenge).toHaveBeenCalledExactlyOnceWith(
            challenge.challengeToken,
        );
        expect(gameStartRedirect).not.toHaveBeenCalled();
    });

    it("should log error if cancelChallenge returns error", async () => {
        const user = userEvent.setup();
        cancelChallengeMock.mockResolvedValue({
            error: { errors: [] },
            data: undefined,
            response: new Response(),
        });

        render(
            <ChallengeNotification
                challenge={challenge}
                removeChallenge={removeChallenge}
            />,
        );

        await user.click(screen.getByTestId("challengeNotificationDecline"));

        expect(
            screen.getByTestId("challengeNotificationError"),
        ).toHaveTextContent("Failed to decline");
        expect(removeChallenge).not.toHaveBeenCalled();
        expect(gameStartRedirect).not.toHaveBeenCalled();
    });

    it("should call acceptChallenge and navigate on success", async () => {
        const user = userEvent.setup();
        render(
            <ChallengeNotification
                challenge={challenge}
                removeChallenge={removeChallenge}
            />,
        );

        await user.click(screen.getByTestId("challengeNotificationAccept"));

        expect(acceptChallenge).toHaveBeenCalledWith({
            path: { challengeToken: challenge.challengeToken },
        });
        expect(removeChallenge).toHaveBeenCalledWith(challenge.challengeToken);
        expect(gameStartRedirect).toHaveBeenCalledExactlyOnceWith(
            gameToken,
            expect.anything(),
        );
    });

    it("should not navigate if acceptChallenge returns error", async () => {
        const user = userEvent.setup();
        acceptChallengeMock.mockResolvedValue({
            error: { errors: [] },
            data: undefined,
            response: new Response(),
        });

        render(
            <ChallengeNotification
                challenge={challenge}
                removeChallenge={removeChallenge}
            />,
        );

        await user.click(screen.getByTestId("challengeNotificationAccept"));

        expect(
            screen.getByTestId("challengeNotificationError"),
        ).toHaveTextContent("Failed to accept");
        expect(removeChallenge).not.toHaveBeenCalled();
        expect(gameStartRedirect).not.toHaveBeenCalled();
    });
});
