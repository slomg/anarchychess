import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    createFakeMove,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createFakePositionHistory } from "@/lib/testUtils/fakers/positionHistoryFaker";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { logicalPoint } from "@/features/point/pointUtils";
import NavigationButtons from "../NavigationButtons";

describe("NavigationButtons", () => {
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        chessboardStore = createChessboardStore();
    });

    it("should update position", async () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });

        const rootPieces = BoardPieces.fromPieces(piece);
        const position1 = createFakePositionProps({
            pieces: BoardPieces.fromPieces({
                ...piece,
                position: logicalPoint({ x: 1, y: 0 }),
                hasMoved: true,
            }),
            move: createFakeMove({
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 1, y: 0 }),
            }),
        });
        const position2 = createFakePositionProps({
            pieces: BoardPieces.fromPieces({
                ...piece,
                position: logicalPoint({ x: 2, y: 0 }),
                hasMoved: true,
            }),
            move: createFakeMove({
                from: logicalPoint({ x: 1, y: 0 }),
                to: logicalPoint({ x: 2, y: 0 }),
            }),
        });

        chessboardStore.setState({
            pieces: rootPieces,
            positionHistory: createFakePositionHistory({
                rootPieces,
                pos: [position1, position2],
            }),
        });

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <NavigationButtons />
            </ChessboardStoreContext.Provider>,
        );

        const startButton = screen.getByTitle("Go to Start");
        const backwardButton = screen.getByTitle("Previous Move");
        const forwardButton = screen.getByTitle("Next Move");
        const endButton = screen.getByTitle("Go to End");

        await user.click(backwardButton);
        expect(chessboardStore.getState().pieces).toEqual(position1.pieces);

        await user.click(backwardButton);
        expect(chessboardStore.getState().pieces).toEqual(rootPieces);

        await user.click(forwardButton);
        expect(chessboardStore.getState().pieces).toEqual(position1.pieces);

        await user.click(endButton);
        expect(chessboardStore.getState().pieces).toEqual(position2.pieces);

        await user.click(startButton);
        expect(chessboardStore.getState().pieces).toEqual(rootPieces);
    });
});
