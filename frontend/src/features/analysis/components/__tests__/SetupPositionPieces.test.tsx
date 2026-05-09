import { act, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import ChessboardLayout from "@/features/chessboard/components/ChessboardLayout";
import { mockBoundingClientRect } from "@/lib/testUtils/mocks/mockDom";
import SetupPositionPieces from "../SetupPositionPieces";
import { GameColor, PieceType } from "@/lib/apiClient";
import { logicalPoint } from "@/features/point/pointUtils";

describe("SetupPositionPieces", () => {
    let store: StoreApi<ChessboardStore>;

    const addSetupModePieceMock = vi.fn();
    const boardRect = {
        width: 768,
        height: 768,
        right: 768,
        bottom: 768,
    } as DOMRect;

    beforeEach(() => {
        store = createChessboardStore();
        store.setState({
            addSetupModePiece: addSetupModePieceMock,
        });

        vi.useFakeTimers({
            toFake: ["requestAnimationFrame"],
            shouldAdvanceTime: true,
        });
    });

    function mockSetupPieceRect(type: PieceType, color: GameColor | null) {
        mockBoundingClientRect({
            [`setupPiece-${type}-${color}`]: {
                x: 50,
                y: 50,
                width: 60,
                height: 60,
            },
            chessboard: boardRect,
        });
    }

    it("should select and deselect the piece image after clicking", async () => {
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessboardLayout />
                <SetupPositionPieces />
            </ChessboardStoreContext.Provider>,
        );

        const setupPiece = screen.getByTestId(
            `setupPiece-${PieceType.QUEEN}-${GameColor.WHITE}`,
        );
        expect(setupPiece).not.toHaveClass("bg-primary");

        await user.click(setupPiece);
        expect(setupPiece).toHaveClass("bg-primary");

        await user.click(setupPiece);
        expect(setupPiece).not.toHaveClass("bg-primary");
    });

    it("should switch selection when clicking a different piece", async () => {
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessboardLayout />
                <SetupPositionPieces />
            </ChessboardStoreContext.Provider>,
        );
        const queen = screen.getByTestId(
            `setupPiece-${PieceType.QUEEN}-${GameColor.WHITE}`,
        );
        const rook = screen.getByTestId(
            `setupPiece-${PieceType.ROOK}-${GameColor.WHITE}`,
        );

        await user.click(queen);
        expect(queen).toHaveClass("bg-primary");
        expect(rook).not.toHaveClass("bg-primary");

        await user.click(rook);
        expect(rook).toHaveClass("bg-primary");
        expect(queen).not.toHaveClass("bg-primary");
    });

    it("should not highlight black piece when white piece of same type is selected", async () => {
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessboardLayout />
                <SetupPositionPieces />
            </ChessboardStoreContext.Provider>,
        );
        const whiteQueen = screen.getByTestId(
            `setupPiece-${PieceType.QUEEN}-${GameColor.WHITE}`,
        );
        const blackQueen = screen.getByTestId(
            `setupPiece-${PieceType.QUEEN}-${GameColor.BLACK}`,
        );

        await user.click(whiteQueen);
        expect(whiteQueen).toHaveClass("bg-primary");
        expect(blackQueen).not.toHaveClass("bg-primary");
    });

    it("should select neutral piece independently from colored pieces", async () => {
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessboardLayout />
                <SetupPositionPieces />
            </ChessboardStoreContext.Provider>,
        );
        const neutralPiece = screen.getByTestId(
            `setupPiece-${PieceType.TRAITOR_ROOK}-null`,
        );
        const whitePiece = screen.getByTestId(
            `setupPiece-${PieceType.QUEEN}-${GameColor.WHITE}`,
        );

        await user.click(neutralPiece);
        expect(neutralPiece).toHaveClass("bg-primary");
        expect(whitePiece).not.toHaveClass("bg-primary");
    });

    it("should select piece when dragged less than threshold", async () => {
        mockSetupPieceRect(PieceType.QUEEN, GameColor.WHITE);

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessboardLayout />
                <SetupPositionPieces />
            </ChessboardStoreContext.Provider>,
        );
        const queen = screen.getByTestId(
            `setupPiece-${PieceType.QUEEN}-${GameColor.WHITE}`,
        );
        const { x, y } = queen.getBoundingClientRect();

        await user.pointer([
            { keys: "[MouseLeft>]", target: queen, coords: { x, y } },
            { coords: { x: x + 39, y } },
            { keys: "[/MouseLeft]" },
        ]);

        expect(queen).toHaveClass("bg-primary");
    });

    it("should not select piece when dragged past threshold", async () => {
        mockSetupPieceRect(PieceType.QUEEN, GameColor.WHITE);

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessboardLayout />
                <SetupPositionPieces />
            </ChessboardStoreContext.Provider>,
        );
        const queen = screen.getByTestId(
            `setupPiece-${PieceType.QUEEN}-${GameColor.WHITE}`,
        );
        const { x, y } = queen.getBoundingClientRect();

        await user.pointer([
            { keys: "[MouseLeft>]", target: queen, coords: { x, y } },
            { coords: { x: x + 41, y } },
            { keys: "[/MouseLeft]" },
        ]);

        expect(queen).not.toHaveClass("bg-primary");
    });

    it("should show ghost image while dragging", async () => {
        mockSetupPieceRect(PieceType.QUEEN, GameColor.WHITE);
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessboardLayout />
                <SetupPositionPieces />
            </ChessboardStoreContext.Provider>,
        );
        const queen = screen.getByTestId(
            `setupPiece-${PieceType.QUEEN}-${GameColor.WHITE}`,
        );
        const { x, y } = queen.getBoundingClientRect();

        await user.pointer({
            keys: "[MouseLeft>]",
            target: queen,
            coords: { x, y },
        });
        const ghost = screen.getByTestId("setupPositionPiecesGhost");
        expect(ghost).not.toBeVisible();

        await user.pointer({ coords: { x: x + 5, y } });
        act(() => vi.runAllTimers());
        expect(ghost).toBeVisible();

        await user.pointer({ keys: "[/MouseLeft]" });
        expect(
            screen.queryByTestId("setupPositionPiecesGhost"),
        ).not.toBeInTheDocument();
    });

    it("should add setup piece with correct piece type and color when dragged onto board", async () => {
        mockSetupPieceRect(PieceType.QUEEN, GameColor.WHITE);
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessboardLayout />
                <SetupPositionPieces />
            </ChessboardStoreContext.Provider>,
        );
        const queen = screen.getByTestId(
            `setupPiece-${PieceType.QUEEN}-${GameColor.WHITE}`,
        );
        const dropPoint = store
            .getState()
            .logicalPointToScreenPoint(logicalPoint({ x: 5, y: 5 }));

        await user.pointer([
            { keys: "[MouseLeft>]", target: queen },
            { coords: dropPoint },
            { keys: "[/MouseLeft]" },
        ]);
        act(() => vi.runAllTimers());

        expect(addSetupModePieceMock).toHaveBeenCalledWith(
            PieceType.QUEEN,
            GameColor.WHITE,
            dropPoint,
        );
    });

    it("should add setup piece with correct piece type and color after board click", async () => {
        mockSetupPieceRect(PieceType.QUEEN, GameColor.WHITE);
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessboardLayout />
                <SetupPositionPieces />
            </ChessboardStoreContext.Provider>,
        );
        const queen = screen.getByTestId(
            `setupPiece-${PieceType.QUEEN}-${GameColor.WHITE}`,
        );
        await user.click(queen);

        const dropPoint = store
            .getState()
            .logicalPointToScreenPoint(logicalPoint({ x: 5, y: 5 }));

        await user.pointer([
            {
                keys: "[MouseLeft]",
                target: screen.getByTestId("chessboard"),
                coords: dropPoint,
            },
        ]);
        act(() => vi.runAllTimers());

        expect(addSetupModePieceMock).toHaveBeenCalledWith(
            PieceType.QUEEN,
            GameColor.WHITE,
            dropPoint,
        );
    });
});
