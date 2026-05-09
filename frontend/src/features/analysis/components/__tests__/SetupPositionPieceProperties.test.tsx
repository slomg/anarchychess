import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import SetupPositionPieceProperties from "../SetupPositionPieceProperties";
import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { PieceType } from "@/lib/apiClient";
import { logicalToAlgebraic } from "@/features/point/pointUtils";

describe("SetupPositionPieceProperties", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should render nothing when no piece is selected", () => {
        store.setState({ selectedPieceId: null });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupPositionPieceProperties />
            </ChessboardStoreContext.Provider>,
        );

        expect(
            screen.queryByTestId("setupPieceProperties"),
        ).not.toBeInTheDocument();
    });

    it("should display the piece type and position in the title", () => {
        const piece = createFakePiece({
            type: PieceType.QUEEN,
        });
        store.setState({
            pieces: BoardPieces.fromPieces(piece),
            selectedPieceId: piece.id,
        });
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupPositionPieceProperties />
            </ChessboardStoreContext.Provider>,
        );

        expect(screen.getByText("Queen")).toBeInTheDocument();
        expect(
            screen.getByText(
                `Piece properties on ${logicalToAlgebraic(piece.position)}`,
            ),
        ).toBeInTheDocument();
    });

    it("should display piece values as default input values", () => {
        const piece = createFakePiece({ stunnedForTurns: 3, hasMoved: true });
        store.setState({
            pieces: BoardPieces.fromPieces(piece),
            selectedPieceId: piece.id,
        });
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupPositionPieceProperties />
            </ChessboardStoreContext.Provider>,
        );

        expect(screen.getByLabelText("Stunned for (plies)")).toHaveValue(3);
        expect(
            screen.getByTestId("setupPiecePropertiesHasMoved"),
        ).toHaveAttribute("data-selected", "true");
    });

    it("should update hasMoved when has moved is toggled", async () => {
        const user = userEvent.setup();
        const piece = createFakePiece({ hasMoved: false });
        store.setState({
            pieces: BoardPieces.fromPieces(piece),
            selectedPieceId: piece.id,
        });
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupPositionPieceProperties />
            </ChessboardStoreContext.Provider>,
        );

        const selector = screen.getByTestId("setupPiecePropertiesHasMoved");
        expect(selector).toHaveAttribute("data-selected", "false");

        await user.click(within(selector).getByTestId("selector-true"));

        expect(selector).toHaveAttribute("data-selected", "true");
        expect(store.getState().pieces.getById(piece.id)?.hasMoved).toBe(true);
    });

    it("should update stunnedForTurns when a number is typed", async () => {
        const user = userEvent.setup();
        const piece = createFakePiece({ stunnedForTurns: 0 });
        store.setState({
            pieces: BoardPieces.fromPieces(piece),
            selectedPieceId: piece.id,
        });
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupPositionPieceProperties />
            </ChessboardStoreContext.Provider>,
        );

        const input = screen.getByLabelText("Stunned for (plies)");
        await user.clear(input);
        await user.type(input, "3");

        expect(store.getState().pieces.getById(piece.id)?.stunnedForTurns).toBe(
            3,
        );
        expect(input).toHaveValue(3);
    });

    it("should not update stunnedForTurns when a letter is typed", async () => {
        const user = userEvent.setup();
        const piece = createFakePiece({ stunnedForTurns: 2 });
        store.setState({
            pieces: BoardPieces.fromPieces(piece),
            selectedPieceId: piece.id,
        });
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupPositionPieceProperties />
            </ChessboardStoreContext.Provider>,
        );

        const input = screen.getByLabelText("Stunned for (plies)");
        await user.type(input, "a");

        expect(store.getState().pieces.getById(piece.id)?.stunnedForTurns).toBe(
            2,
        );
        expect(input).toHaveValue(2);
    });

    it("should decrement stunnedForTurns when decrement is clicked", async () => {
        const user = userEvent.setup();
        const piece = createFakePiece({ stunnedForTurns: 2 });
        store.setState({
            pieces: BoardPieces.fromPieces(piece),
            selectedPieceId: piece.id,
        });
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupPositionPieceProperties />
            </ChessboardStoreContext.Provider>,
        );

        await user.click(
            screen.getByTestId("setupPiecePropertiesDecrementStunned"),
        );

        expect(store.getState().pieces.getById(piece.id)?.stunnedForTurns).toBe(
            1,
        );
        expect(screen.getByLabelText("Stunned for (plies)")).toHaveValue(1);
    });

    it("should not decrement stunnedForTurns below 0", async () => {
        const piece = createFakePiece({ stunnedForTurns: 0 });
        store.setState({
            pieces: BoardPieces.fromPieces(piece),
            selectedPieceId: piece.id,
        });

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupPositionPieceProperties />
            </ChessboardStoreContext.Provider>,
        );

        await user.click(
            screen.getByTestId("setupPiecePropertiesDecrementStunned"),
        );

        expect(store.getState().pieces.getById(piece.id)?.stunnedForTurns).toBe(
            0,
        );
        expect(screen.getByLabelText("Stunned for (plies)")).toHaveValue(0);
    });

    it("should increment stunnedForTurns when increment is clicked", async () => {
        const piece = createFakePiece({ stunnedForTurns: 2 });
        store.setState({
            pieces: BoardPieces.fromPieces(piece),
            selectedPieceId: piece.id,
        });

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupPositionPieceProperties />
            </ChessboardStoreContext.Provider>,
        );

        await user.click(
            screen.getByTestId("setupPiecePropertiesIncrementStunned"),
        );

        expect(store.getState().pieces.getById(piece.id)?.stunnedForTurns).toBe(
            3,
        );
        expect(screen.getByLabelText("Stunned for (plies)")).toHaveValue(3);
    });
});
