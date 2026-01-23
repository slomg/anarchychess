import { render, screen } from "@testing-library/react";
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
import HighlightedLegalMovesRenderer from "../HighlightedLegalMove";
import { logicalPoint } from "@/features/point/pointUtils";
import { IntermediateSquare } from "../../lib/types";
import BoardPieces from "../../lib/boardPieces";
import LegalMoves from "../../lib/legalMoves";

describe("HighlightedLegalMovesRenderer", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should highlight unique points from 'to' and 'triggers'", () => {
        const piece = createFakePiece();
        const move1To = logicalPoint({ x: 3, y: 3 });
        const move2To = logicalPoint({ x: 4, y: 4 });
        const trigger1 = logicalPoint({ x: 5, y: 5 });
        const trigger2 = move1To; // same as move1To

        const move1 = createFakeMove({
            from: piece.position,
            to: move1To,
            triggers: [trigger1, trigger2],
        });
        const move2 = createFakeMove({
            from: piece.position,
            to: move2To,
            triggers: [],
        });

        const legalMoves = new LegalMoves([move1, move2]);

        const { setLatestLegalMoves, selectPiece } = store.getState();
        store.setState({ pieces: BoardPieces.fromPieces(piece) });
        setLatestLegalMoves(legalMoves);
        selectPiece(piece.id);

        render(
            <ChessboardStoreContext.Provider value={store}>
                <HighlightedLegalMovesRenderer />
            </ChessboardStoreContext.Provider>,
        );

        const squares = screen.getAllByTestId("highlightedLegalMove");
        const expectedPoints = [move1To, move2To, trigger1];

        expectedPoints.forEach((point) => {
            const square = squares.find(
                (el) =>
                    el.getAttribute("data-position") ===
                    `${point.x},${point.y}`,
            );
            expect(square).toBeInTheDocument();
        });
    });

    it("should highlight the first intermediate instead of 'to'", () => {
        const piece = createFakePiece();
        const intermediate: IntermediateSquare = {
            position: logicalPoint({ x: 1, y: 1 }),
            isCapture: false,
        };
        const destination = logicalPoint({ x: 2, y: 2 });
        const move = createFakeMove({
            from: piece.position,
            to: destination,
            intermediates: [intermediate],
        });

        const legalMoves = new LegalMoves([move]);

        const { setLatestLegalMoves, selectPiece } = store.getState();
        store.setState({ pieces: BoardPieces.fromPieces(piece) });
        setLatestLegalMoves(legalMoves);
        selectPiece(piece.id);

        render(
            <ChessboardStoreContext.Provider value={store}>
                <HighlightedLegalMovesRenderer />
            </ChessboardStoreContext.Provider>,
        );

        const squares = screen.getAllByTestId("highlightedLegalMove");
        const expectedPoints = [intermediate.position];
        expectedPoints.forEach((point) => {
            const square = squares.find(
                (el) =>
                    el.getAttribute("data-position") ===
                    `${point.x},${point.y}`,
            );
            expect(square).toBeInTheDocument();
        });
    });
});
