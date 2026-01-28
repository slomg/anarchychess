import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    createFakeBoardPieces,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";

import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import { pointToStr } from "@/features/point/pointUtils";
import PieceRenderer from "../PieceRenderer";

describe("PieceRenderer", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should render all animating pieces when animatingPieces exists", () => {
        const animatingPieces = createFakeBoardPieces(3);

        store.setState({ animatingPieces, pieces: createFakeBoardPieces(4) });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <PieceRenderer />
            </ChessboardStoreContext.Provider>,
        );

        const renderedPieces = screen.getAllByTestId("piece");
        expect(renderedPieces).toHaveLength(animatingPieces.size);

        Array.from(animatingPieces.values()).forEach((piece) => {
            const element = renderedPieces.find(
                (el) =>
                    el.getAttribute("data-position") ===
                    pointToStr(piece.position),
            );
            expect(element).toBeInTheDocument();
        });
    });

    it("should render all pieces when animatingPieces doesn't exist", () => {
        const pieces = createFakeBoardPieces();

        store.setState({ animatingPieces: null, pieces });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <PieceRenderer />
            </ChessboardStoreContext.Provider>,
        );

        const renderedPieces = screen.getAllByTestId("piece");
        expect(renderedPieces).toHaveLength(pieces.size);

        Array.from(pieces.values()).forEach((piece) => {
            const element = renderedPieces.find(
                (el) =>
                    el.getAttribute("data-position") ===
                    pointToStr(piece.position),
            );
            expect(element).toBeInTheDocument();
        });
    });

    it("should render removing pieces", () => {
        const piece1 = createFakePiece();
        const piece2 = createFakePiece();
        const removingPieces = new Map([
            [piece1.id, piece1],
            [piece2.id, piece2],
        ]);
        store.setState({ removingPieces });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <PieceRenderer />
            </ChessboardStoreContext.Provider>,
        );

        const renderedRemovingPieces = screen.getAllByTestId("removingPiece");
        expect(renderedRemovingPieces).toHaveLength(removingPieces.size);

        Array.from(removingPieces.values()).forEach((piece) => {
            const element = renderedRemovingPieces.find(
                (el) =>
                    el.getAttribute("data-position") ===
                    pointToStr(piece.position),
            );
            expect(element).toBeInTheDocument();
        });
    });
});
