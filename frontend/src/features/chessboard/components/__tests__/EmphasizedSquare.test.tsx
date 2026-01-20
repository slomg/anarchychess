import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";

import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import EmphasizedSquaresRenderer from "../EmphasizedSquare";
import { logicalPoint } from "@/features/point/pointUtils";
import LegalMoves from "../../lib/legalMoves";
import { createFakeMove } from "@/lib/testUtils/fakers/chessboardFakers";

describe("EmphasizedSquaresRenderer", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should render all emphasized squares at the correct positions", () => {
        const squares = [
            logicalPoint({ x: 1, y: 2 }),
            logicalPoint({ x: 3, y: 4 }),
        ];

        const legalMoves = new LegalMoves();
        for (const square of squares) {
            legalMoves.addMove(
                createFakeMove({ from: square, emphasizeSquare: true }),
            );
        }

        store.setState({
            getViewedPositionLegalMoves: () => legalMoves,
        });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <EmphasizedSquaresRenderer />
            </ChessboardStoreContext.Provider>,
        );

        const renderedSquares = screen.getAllByTestId("emphasizedSquare");
        expect(renderedSquares).toHaveLength(squares.length);

        squares.forEach((sq) => {
            const square = renderedSquares.find(
                (el) => el.getAttribute("data-position") === `${sq.x},${sq.y}`,
            );
            expect(square).toBeInTheDocument();
        });
    });
});
