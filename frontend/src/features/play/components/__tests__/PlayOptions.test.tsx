import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { createFakeOngoingGame } from "@/lib/testUtils/fakers/ongoingGameFaker";
import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";
import useLobbyStore from "@/features/lobby/stores/lobbyStore";
import { PoolType } from "@/lib/apiClient";
import PlayOptions from "../PlayOptions";
import constants from "@/lib/constants";

vi.mock("@/features/lobby/hooks/useLobbyHub");
vi.mock("js-cookie");

describe("PlayOptions", () => {
    beforeEach(() => {
        useLobbyStore.setState(useLobbyStore.getInitialState());

        mockJsCookie({ [constants.COOKIES.IS_LOGGED_IN]: "true" });
    });

    it("should render the heading and main container", () => {
        render(<PlayOptions />);

        expect(screen.getByText("Play Anarchy Chess")).toBeInTheDocument();
        expect(screen.getByTestId("playOptions")).toBeInTheDocument();
    });

    it("should show PoolToggle when authenticated", () => {
        render(<PlayOptions />);

        expect(screen.getByTestId("poolToggle")).toBeInTheDocument();
    });

    it("should hide PoolToggle when unauthenticated", () => {
        mockJsCookie({});

        render(<PlayOptions />);

        expect(screen.queryByTestId("poolToggle")).not.toBeInTheDocument();
    });

    it("should render casual PoolButtons when isRated is false", () => {
        render(<PlayOptions />);

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).not.toBeVisible();
    });

    it("should render rated PoolButtons when isRated is true", async () => {
        const user = userEvent.setup();
        render(<PlayOptions />);

        const poolToggle = screen.getByTestId("poolToggle");

        await user.click(poolToggle);

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).not.toBeVisible();
    });

    it("should persist pool type across mounts", async () => {
        const user = userEvent.setup();

        const { unmount } = render(<PlayOptions />);
        const poolToggle = screen.getByTestId("poolToggle");

        await user.click(poolToggle);
        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).toBeVisible();

        unmount();
        render(<PlayOptions />);

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).not.toBeVisible();
    });

    it("should reset pool type to casual when logging out", async () => {
        const user = userEvent.setup();

        const { unmount } = render(<PlayOptions />);

        await user.click(screen.getByTestId("poolToggle"));

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).not.toBeVisible();

        mockJsCookie({});
        unmount();
        render(<PlayOptions />);

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).not.toBeVisible();
    });

    it("should open challenge popup when clicking on challenge a friend", async () => {
        const user = userEvent.setup();
        render(<PlayOptions />);

        await user.click(screen.getByText("Challenge a Friend"));

        expect(screen.getByTestId("challengePopup")).toBeInTheDocument();
    });

    it("should not display ongoing games button when there are no ongoing games", () => {
        useLobbyStore.setState({ ongoingGames: new Map() });

        render(<PlayOptions />);

        expect(
            screen.queryByText("Resume Ongoing Games"),
        ).not.toBeInTheDocument();
    });

    it("should open ongoing games popup when clicking on resumt ongoing games", async () => {
        useLobbyStore.setState({
            ongoingGames: new Map([
                ["token", createFakeOngoingGame({ gameToken: "token" })],
            ]),
        });

        const user = userEvent.setup();
        render(<PlayOptions />);

        await user.click(screen.getByText("Resume Ongoing Games"));

        expect(screen.getByTestId("ongoingGamesPopup")).toBeInTheDocument();
    });
});
