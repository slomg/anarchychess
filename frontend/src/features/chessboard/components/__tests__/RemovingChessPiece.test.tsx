import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import RemovingChessPiece from "../RemovingChessPiece";
import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";
import { getPieceImage } from "../../lib/pieceImage";
import { pointToStr } from "@/features/point/pointUtils";

describe("RemovingChessPiece", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should be in the correct piece", () => {
        const piece = createFakePiece();
        store.setState({
            removingPieces: new Map([
                ["1", createFakePiece()],
                [piece.id, piece],
                ["3", createFakePiece()],
            ]),
        });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <RemovingChessPiece id={piece.id} />
            </ChessboardStoreContext.Provider>,
        );

        const element = screen.getByTestId("removingPiece");
        expect(element).toHaveStyle(`
                background-image: url("${getPieceImage(piece.type, piece.color)}");
            `);
        expect(element).toHaveAttribute(
            "data-position",
            pointToStr(piece.position),
        );
    });
});
