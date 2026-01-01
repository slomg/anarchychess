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
import { mockScrollTo } from "@/lib/testUtils/mocks/mockDom";
import { logicalPoint } from "@/features/point/pointUtils";
import MoveHistoryTable from "../MoveHistoryTable";
import BoardPieces from "../../../lib/boardPieces";
import { GameColor } from "@/lib/apiClient";

describe("MoveHistoryTable", () => {
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        mockScrollTo();
        chessboardStore = createChessboardStore();
    });

    function renderWithCtx() {
        return render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryTable />
            </ChessboardStoreContext.Provider>,
        );
    }

    it("should render an empty table when there are no moves", () => {
        renderWithCtx();
        const rows = screen.queryAllByRole("row");
        expect(rows.length).toBe(0);
    });

    it("should render a single row when there is one move", () => {
        chessboardStore.setState({
            positionHistory: createFakePositionHistory({
                pos: [createFakePositionProps({ san: "e4" })],
            }),
        });

        renderWithCtx();

        expect(screen.getByText("1.")).toBeInTheDocument();
        expect(screen.getByText("e4")).toBeInTheDocument();
    });

    it("should render multiple rows for multiple moves", () => {
        chessboardStore.setState({
            positionHistory: createFakePositionHistory({
                pos: [
                    createFakePositionProps({ san: "e4" }),
                    createFakePositionProps({ san: "e5" }),
                    createFakePositionProps({ san: "Nf3" }),
                    createFakePositionProps({ san: "Nc6" }),
                ],
            }),
        });

        renderWithCtx();

        expect(screen.getByText("1.")).toBeInTheDocument();
        expect(screen.getByText("2.")).toBeInTheDocument();
        expect(screen.getByText("e4")).toBeInTheDocument();
        expect(screen.getByText("e5")).toBeInTheDocument();
        expect(screen.getByText("Nf3")).toBeInTheDocument();
        expect(screen.getByText("Nc6")).toBeInTheDocument();
    });

    it("should apply alternating background color class for odd rows", () => {
        chessboardStore.setState({
            positionHistory: createFakePositionHistory({
                pos: [
                    createFakePositionProps({ san: "e4" }),
                    createFakePositionProps({ san: "e5" }),
                    createFakePositionProps({ san: "Nf3" }),
                    createFakePositionProps({ san: "Nf6" }),
                ],
            }),
        });

        renderWithCtx();

        const rows = screen.getAllByRole("row");
        expect(rows.length).toBe(2);

        expect(rows[0].className).not.toContain("bg-white/10");
        expect(rows[1].className).toContain("bg-white/10");
    });

    it("should update position using arrow keys", async () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });

        const rootPieces = BoardPieces.fromPieces(piece);
        const position1 = createFakePositionProps({
            pieces: BoardPieces.fromPieces({
                ...piece,
                position: logicalPoint({ x: 1, y: 0 }),
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
        renderWithCtx();

        // step backward 2 -> 1
        await user.keyboard("{ArrowLeft}");
        expect(chessboardStore.getState().pieces).toEqual(position1.pieces);

        // step backward 1 -> root
        await user.keyboard("{ArrowLeft}");
        expect(chessboardStore.getState().pieces).toEqual(rootPieces);

        // step forward root -> 1
        await user.keyboard("{ArrowRight}");
        expect(chessboardStore.getState().pieces).toEqual(position1.pieces);

        // jump to end
        await user.keyboard("{ArrowDown}");
        expect(chessboardStore.getState().pieces).toEqual(position2.pieces);

        // jump to start
        await user.keyboard("{ArrowUp}");
        expect(chessboardStore.getState().pieces).toEqual(rootPieces);
    });

    it("should update position when clicking on a move", async () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });

        const rootPieces = BoardPieces.fromPieces(piece);
        const position1 = createFakePositionProps({
            san: "e4",
            pieces: BoardPieces.fromPieces({
                ...piece,
                position: logicalPoint({ x: 1, y: 0 }),
            }),
            move: createFakeMove({
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 1, y: 0 }),
            }),
        });
        const position2 = createFakePositionProps({
            san: "e5",
            pieces: BoardPieces.fromPieces({
                ...piece,
                position: logicalPoint({ x: 2, y: 0 }),
            }),
            move: createFakeMove({
                from: logicalPoint({ x: 1, y: 0 }),
                to: logicalPoint({ x: 2, y: 0 }),
            }),
        });
        const position3 = createFakePositionProps({
            san: "e6",
            pieces: BoardPieces.fromPieces({
                ...piece,
                position: logicalPoint({ x: 3, y: 0 }),
            }),
            move: createFakeMove({
                from: logicalPoint({ x: 2, y: 0 }),
                to: logicalPoint({ x: 3, y: 0 }),
            }),
        });

        chessboardStore.setState({
            positionHistory: createFakePositionHistory({
                rootPieces,
                pos: [position1, position2, position3],
            }),
        });

        const user = userEvent.setup();
        renderWithCtx();

        await user.click(screen.getByText("e4"));
        expect(chessboardStore.getState().pieces).toEqual(position1.pieces);

        await user.click(screen.getByText("e5"));
        expect(chessboardStore.getState().pieces).toEqual(position2.pieces);

        await user.click(screen.getByText("e6"));
        expect(chessboardStore.getState().pieces).toEqual(position3.pieces);
    });

    it("should flip the board when the flip board icon is clicked", async () => {
        chessboardStore.setState({ viewingFrom: GameColor.WHITE });
        const user = userEvent.setup();

        renderWithCtx();

        const flipIcon = screen.getByTitle("Flip Board");
        await user.click(flipIcon);

        expect(chessboardStore.getState().viewingFrom).toBe(GameColor.BLACK);
    });
});
