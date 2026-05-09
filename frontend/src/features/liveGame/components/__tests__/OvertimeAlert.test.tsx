import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";
import { act } from "react";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { createFakeBoardPieces } from "@/lib/testUtils/fakers/chessboardFakers";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { GameClientEvents, useGameEvent } from "../../hooks/useGameHub";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { logicalPoint, pointToStr } from "@/features/point/pointUtils";
import LiveChessStoreContext from "../../contexts/liveChessContext";
import OvertimeAlert from "../OvertimeAlert";

vi.mock("@/features/liveGame/hooks/useGameHub");

describe("OvertimeAlert", () => {
    const gameEventHandlers: EventHandlers<GameClientEvents> = {};
    const useGameEventMock = vi.mocked(useGameEvent);

    let liveStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        liveStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        useGameEventMock.mockImplementation((_, event, handler) => {
            gameEventHandlers[event] = handler;
        });
    });

    it("should not render anything before an event", () => {
        render(
            <LiveChessStoreContext.Provider value={liveStore}>
                <ChessboardStoreContext.Provider value={chessboardStore}>
                    <OvertimeAlert />
                </ChessboardStoreContext.Provider>
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.queryByTestId("overtimeSquare")).not.toBeInTheDocument();
    });

    it("should emphasize the square when the ply number is matching", () => {
        const positionHistory = new PositionHistory({
            pieces: createFakeBoardPieces(),
        });
        positionHistory.addNextPosition(createFakePositionProps());
        positionHistory.addNextPosition(createFakePositionProps());
        chessboardStore.setState({ positionHistory });
        const removeFrom = logicalPoint({ x: 1, y: 2 });

        render(
            <LiveChessStoreContext.Provider value={liveStore}>
                <ChessboardStoreContext.Provider value={chessboardStore}>
                    <OvertimeAlert />
                </ChessboardStoreContext.Provider>
            </LiveChessStoreContext.Provider>,
        );

        act(() => {
            gameEventHandlers.ReceiveNextOvertimeAsync?.(
                positionHistory.mainPlyCount,
                removeFrom,
            );
        });

        const square = screen.getByTestId("overtimeSquare");
        expect(square).toBeInTheDocument();
        expect(square).toHaveAttribute("data-position", pointToStr(removeFrom));
    });

    it("should not emphasize the square when the ply number is not matching", () => {
        const positionHistory = new PositionHistory({
            pieces: createFakeBoardPieces(),
        });
        positionHistory.addNextPosition(createFakePositionProps());
        positionHistory.addNextPosition(createFakePositionProps());
        positionHistory.addNextPosition(createFakePositionProps());
        chessboardStore.setState({ positionHistory });
        const removeFrom = logicalPoint({ x: 5, y: 4 });

        render(
            <LiveChessStoreContext.Provider value={liveStore}>
                <ChessboardStoreContext.Provider value={chessboardStore}>
                    <OvertimeAlert />
                </ChessboardStoreContext.Provider>
            </LiveChessStoreContext.Provider>,
        );

        act(() => {
            gameEventHandlers.ReceiveNextOvertimeAsync?.(
                positionHistory.mainPlyCount - 1,
                removeFrom,
            );
        });

        expect(screen.queryByTestId("overtimeSquare")).not.toBeInTheDocument();
    });

    it("should not emphasize the square when the game is over", () => {
        render(
            <LiveChessStoreContext.Provider value={liveStore}>
                <ChessboardStoreContext.Provider value={chessboardStore}>
                    <OvertimeAlert />
                </ChessboardStoreContext.Provider>
            </LiveChessStoreContext.Provider>,
        );

        act(() => {
            liveStore.setState({ resultData: createFakeGameResultData() });
            gameEventHandlers.ReceiveNextOvertimeAsync?.(
                0,
                logicalPoint({ x: 0, y: 1 }),
            );
        });

        expect(screen.queryByTestId("overtimeSquare")).not.toBeInTheDocument();
    });

    it("should replace the last alert after a new event", () => {
        const firstPoint = logicalPoint({ x: 1, y: 2 });
        const secondPoint = logicalPoint({ x: 3, y: 4 });

        render(
            <LiveChessStoreContext.Provider value={liveStore}>
                <ChessboardStoreContext.Provider value={chessboardStore}>
                    <OvertimeAlert />
                </ChessboardStoreContext.Provider>
            </LiveChessStoreContext.Provider>,
        );

        act(() => {
            gameEventHandlers.ReceiveNextOvertimeAsync?.(0, firstPoint);
        });

        let square = screen.getByTestId("overtimeSquare");
        expect(square).toBeInTheDocument();
        expect(square).toHaveAttribute("data-position", pointToStr(firstPoint));

        act(() => {
            gameEventHandlers.ReceiveNextOvertimeAsync?.(0, secondPoint);
        });

        square = screen.getByTestId("overtimeSquare");
        expect(square).toBeInTheDocument();
        expect(square).toHaveAttribute(
            "data-position",
            pointToStr(secondPoint),
        );
    });
});
