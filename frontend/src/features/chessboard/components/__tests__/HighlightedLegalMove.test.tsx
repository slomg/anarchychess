import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { logicalPoint } from "@/features/point/pointUtils";
import HighlightedLegalMovesRenderer from "../HighlightedLegalMove";

describe("HighlightedLegalMovesRenderer", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should render all highlighted legal moves at the correct positions", () => {
        const moves = [
            logicalPoint({ x: 1, y: 2 }),
            logicalPoint({ x: 3, y: 4 }),
        ];
        store.setState({ highlightedLegalMoves: moves });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <HighlightedLegalMovesRenderer />
            </ChessboardStoreContext.Provider>,
        );

        const squares = screen.getAllByTestId("highlightedLegalMove");
        expect(squares).toHaveLength(moves.length);

        moves.forEach((move) => {
            const square = squares.find(
                (el) =>
                    el.getAttribute("data-position") === `${move.x},${move.y}`,
            );
            expect(square).toBeInTheDocument();
        });
    });
});
