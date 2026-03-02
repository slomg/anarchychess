import { act, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import { GameColor, GameResult } from "@/lib/apiClient";
import GameOverPopup from "../GameOverPopup";

vi.mock("@/features/lobby/hooks/useLobbyHub");
vi.mock("@/features/liveGame/hooks/useGameHub");

describe("GameOverPopup", () => {
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createLiveChessStore(
            createFakeLiveChessStoreProps({ viewerColor: GameColor.WHITE }),
        );
    });

    it("should only render popup once result data is set", async () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.queryByTestId("gameOverPopup")).not.toBeInTheDocument();

        act(() =>
            store.setState({
                resultData: {
                    result: GameResult.ABORTED,
                    resultDescription: "test",
                },
            }),
        );

        expect(screen.getByTestId("gameOverPopup")).toBeInTheDocument();
    });

    it("should show victory title and rating changes for white win", async () => {
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by King Capture",
                whiteRatingChange: 12,
                blackRatingChange: -10,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByTestId("gameOverPopup")).toBeInTheDocument();
        expect(screen.getByText("VICTORY")).toBeInTheDocument();
        expect(
            screen.getByText("White Won by King Capture"),
        ).toBeInTheDocument();
        expect(screen.getByText("+12")).toBeInTheDocument();
        expect(screen.getByText("-10")).toBeInTheDocument();
    });

    it("should show 'YOU LOST' when black wins and player is white", async () => {
        store.setState({
            resultData: {
                result: GameResult.BLACK_WIN,
                resultDescription: "Black Won on Time",
                whiteRatingChange: -15,
                blackRatingChange: +18,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText("YOU LOST")).toBeInTheDocument();
        expect(screen.getByText("Black Won on Time")).toBeInTheDocument();
    });

    it("should show DRAW title and result description", async () => {
        store.setState({
            resultData: {
                result: GameResult.DRAW,
                resultDescription: "Draw by Stalemate",
                whiteRatingChange: 0,
                blackRatingChange: 0,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText("DRAW")).toBeInTheDocument();
        expect(screen.getByText("Draw by Stalemate")).toBeInTheDocument();
    });

    it("should show ABORTED title if game was aborted", async () => {
        store.setState({
            resultData: {
                result: GameResult.ABORTED,
                resultDescription: "Game Aborted",
                whiteRatingChange: 0,
                blackRatingChange: 0,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText("ABORTED")).toBeInTheDocument();
        expect(screen.getByText("Game Aborted")).toBeInTheDocument();
    });

    it("should close when requested", async () => {
        const user = userEvent.setup();
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Resignation",
                whiteRatingChange: 10,
                blackRatingChange: -8,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup />
            </LiveChessStoreContext.Provider>,
        );

        await user.click(screen.getByTestId("closePopup"));
        expect(screen.queryByTestId("gameOverPopup")).not.toBeInTheDocument();
    });

    it("should show 'GAME OVER' when viewer is a spectator", async () => {
        store.setState({
            viewer: { playerColor: null, userId: crypto.randomUUID() },
            resultData: {
                result: GameResult.BLACK_WIN,
                resultDescription: "Black Won on Time",
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByTestId("gameOverPopup")).toBeInTheDocument();
        expect(screen.getByText("GAME OVER")).toBeInTheDocument();
        expect(screen.getByText("Black Won on Time")).toBeInTheDocument();
    });

    it("should render custom controls when provided", async () => {
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won",
            },
        });

        const CustomControl = () => (
            <button data-testid="customControl">test</button>
        );

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup controls={<CustomControl />} />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByTestId("gameOverPopup")).toBeInTheDocument();

        expect(screen.getByTestId("customControl")).toBeInTheDocument();
        expect(screen.getByText("test")).toBeInTheDocument();
    });
});
