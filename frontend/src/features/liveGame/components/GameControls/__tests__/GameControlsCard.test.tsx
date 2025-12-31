import { render, screen } from "@testing-library/react";

import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import GameControlsCard from "../GameControlsCard";
import userEvent from "@testing-library/user-event";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import { StoreApi } from "zustand";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { GameResult } from "@/lib/apiClient";

import { useGameEmitter } from "@/features/liveGame/hooks/useGameHub";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createNFakePositionHistory } from "@/lib/testUtils/fakers/positionHistoryFaker";

vi.mock("@/features/liveGame/hooks/useGameHub");
vi.mock("@/features/lobby/hooks/useLobbyHub");

describe("GameControlsCard", () => {
    const useGameEmitterMock = vi.mocked(useGameEmitter);
    const sendGameEventMock = vi.fn();

    let liveStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        liveStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        useGameEmitterMock.mockReturnValue(sendGameEventMock);
    });

    function renderWithCtx() {
        return render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <LiveChessStoreContext.Provider value={liveStore}>
                    <GameControlsCard />
                </LiveChessStoreContext.Provider>
            </ChessboardStoreContext.Provider>,
        );
    }

    it("should first render LiveGameControls with Abort", () => {
        chessboardStore.setState({
            positionHistory: createNFakePositionHistory(0),
        });

        renderWithCtx();

        expect(screen.getByTitle(/Abort/i)).toBeInTheDocument();
        expect(screen.getByTitle(/Draw/i)).toBeInTheDocument();
    });

    it("should render Resign if moveHistory has 2+ moves", () => {
        chessboardStore.setState({
            positionHistory: createNFakePositionHistory(2),
        });

        renderWithCtx();

        expect(screen.getByTitle(/Resign/i)).toBeInTheDocument();
        expect(screen.getByTitle(/Draw/i)).toBeInTheDocument();
    });

    it("should render GameOverControls when resultData exists", () => {
        liveStore.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "white won",
            },
        });

        renderWithCtx();

        expect(screen.getByText(/New Game/i)).toBeInTheDocument();
        expect(screen.getByText(/Rematch/i)).toBeInTheDocument();
    });

    it("should call sendGameEvent when clicking Abort", async () => {
        const user = userEvent.setup();

        chessboardStore.setState({
            positionHistory: createNFakePositionHistory(1),
        });

        renderWithCtx();

        await user.click(screen.getByTitle(/Abort/i));
        expect(sendGameEventMock).toHaveBeenCalledWith(
            "EndGameAsync",
            liveStore.getState().gameToken,
        );
    });
});
