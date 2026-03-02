import { GameColor, GameResult } from "@/lib/apiClient";

import LiveChessStoreContext from "../../contexts/liveChessContext";
import LiveGameOverPopup from "../LiveGameOverPopup";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { StoreApi } from "zustand";
import { render, screen } from "@testing-library/react";

vi.mock("@/features/lobby/hooks/useLobbyHub");
vi.mock("@/features/liveGame/hooks/useGameHub");

describe("LiveGameOverPopup", () => {
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createLiveChessStore(
            createFakeLiveChessStoreProps({ viewerColor: GameColor.WHITE }),
        );
    });

    it("should render NEW GAME and REMATCH buttons", async () => {
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
                <LiveGameOverPopup />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText("NEW GAME")).toBeInTheDocument();
        expect(screen.getByText("REMATCH")).toBeInTheDocument();
    });

    it("should not render rematch button when viewer is a spectator", () => {
        store.setState({
            viewer: { playerColor: null, userId: crypto.randomUUID() },
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "test",
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <LiveGameOverPopup />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.queryByTestId("REMATCH")).not.toBeInTheDocument();
    });
});
