import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    createFakeBoardPieces,
    createFakeMove,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createFakePositionHistory } from "@/lib/testUtils/fakers/positionHistoryFaker";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { mockScrollTo } from "@/lib/testUtils/mocks/mockDom";
import { logicalPoint } from "@/features/point/pointUtils";
import MoveHistoryRows from "../MoveHistoryRows";
import { GameColor } from "@/lib/apiClient";

describe("MoveHistoryRows", () => {
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        mockScrollTo();
        chessboardStore = createChessboardStore();
    });

    it("should render an empty table when there are no moves", () => {
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryRows />
            </ChessboardStoreContext.Provider>,
        );

        const rows = screen.queryAllByTestId("moveRow");
        expect(rows).toHaveLength(0);
    });

    it("should render a single row when there is one move", () => {
        chessboardStore.setState({
            positionHistory: createFakePositionHistory({
                pos: [createFakePositionProps({ san: "e4" })],
            }),
        });

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryRows />
            </ChessboardStoreContext.Provider>,
        );

        expect(screen.getByTestId("moveHistoryRows")).toHaveTextContent("1.e4");
    });

    it("should render multiple rows for multiple moves", () => {
        chessboardStore.setState({
            positionHistory: createFakePositionHistory({
                pos: [
                    createFakePositionProps({
                        san: "e4",
                        sideToMove: GameColor.BLACK,
                    }),
                    createFakePositionProps({
                        san: "e5",
                        sideToMove: GameColor.WHITE,
                    }),
                    createFakePositionProps({
                        san: "Nf3",
                        sideToMove: GameColor.BLACK,
                    }),
                    createFakePositionProps({
                        san: "Nc6",
                        sideToMove: GameColor.WHITE,
                    }),
                ],
            }),
        });

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryRows />
            </ChessboardStoreContext.Provider>,
        );

        expect(screen.getByTestId("moveHistoryRows")).toHaveTextContent(
            "1.e4 e5 2.Nf3 Nc6".replaceAll(" ", ""),
        );
    });

    it("should handle black to move as root", () => {
        const positionHistory = new PositionHistory({
            pieces: createFakeBoardPieces(),
            sideToMove: GameColor.BLACK,
        });

        positionHistory.addNextPosition(
            createFakePositionProps({ san: "e5", sideToMove: GameColor.WHITE }),
        );
        positionHistory.addNextPosition(
            createFakePositionProps({
                san: "Nf3",
                sideToMove: GameColor.BLACK,
            }),
        );

        chessboardStore.setState({ positionHistory });

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryRows />
            </ChessboardStoreContext.Provider>,
        );

        expect(screen.getByTestId("moveHistoryRows")).toHaveTextContent(
            "1.e5 2.Nf3".replaceAll(" ", ""),
        );
    });

    it("should render root sub variations", () => {
        const positionHistory = new PositionHistory({
            pieces: createFakeBoardPieces(),
        });
        const pos1 = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e4", sideToMove: GameColor.BLACK }),
        );
        positionHistory.goToStart();
        positionHistory.addNextPosition(
            createFakePositionProps({ san: "c4", sideToMove: GameColor.BLACK }),
        );
        positionHistory.goToPosition(pos1.positionId);
        positionHistory.addNextPosition(
            createFakePositionProps({ san: "e5", sideToMove: GameColor.WHITE }),
        );
        positionHistory.addNextPosition(
            createFakePositionProps({
                san: "Nf3",
                sideToMove: GameColor.BLACK,
            }),
        );
        chessboardStore.setState({ positionHistory });

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryRows />
            </ChessboardStoreContext.Provider>,
        );

        expect(screen.getByTestId("moveVariations")).toHaveTextContent("1.c4");
        expect(screen.getByTestId("moveHistoryRows")).toHaveTextContent(
            "1.e4 e5 1.c4 2.Nf3".replaceAll(" ", ""),
        );
    });

    it("should render sub variations for the on white position", () => {
        const positionHistory = new PositionHistory({
            pieces: createFakeBoardPieces(),
        });

        const whitePos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e4", sideToMove: GameColor.BLACK }),
        );
        positionHistory.addNextPosition(
            createFakePositionProps({ san: "e5", sideToMove: GameColor.WHITE }),
        );
        positionHistory.goToPosition(whitePos.positionId);
        positionHistory.addNextPosition(
            createFakePositionProps({ san: "c5", sideToMove: GameColor.WHITE }),
        );
        chessboardStore.setState({ positionHistory });

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryRows />
            </ChessboardStoreContext.Provider>,
        );

        expect(screen.getByTestId("moveVariations")).toHaveTextContent(
            "1...c5",
        );
        expect(screen.getByTestId("moveHistoryRows")).toHaveTextContent(
            "1.e4 e5 1...c5".replaceAll(" ", ""),
        );
    });

    it("should render sub variations for the black position on the next move", () => {
        const positionHistory = new PositionHistory({
            pieces: createFakeBoardPieces(),
        });

        positionHistory.addNextPosition(
            createFakePositionProps({ san: "e4", sideToMove: GameColor.BLACK }),
        );
        const blackPos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e5", sideToMove: GameColor.WHITE }),
        );
        positionHistory.addNextPosition(
            createFakePositionProps({
                san: "Nf3",
                sideToMove: GameColor.BLACK,
            }),
        );
        positionHistory.goToPosition(blackPos.positionId);
        positionHistory.addNextPosition(
            createFakePositionProps({
                san: "Nc3",
                sideToMove: GameColor.BLACK,
            }),
        );
        chessboardStore.setState({ positionHistory });

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryRows />
            </ChessboardStoreContext.Provider>,
        );

        expect(screen.getByTestId("moveVariations")).toHaveTextContent("2.Nc3");
        expect(screen.getByTestId("moveHistoryRows")).toHaveTextContent(
            "1.e4 e5 2.Nf3 2.Nc3".replaceAll(" ", ""),
        );
    });

    it("should apply alternating background color class for odd rows", () => {
        chessboardStore.setState({
            positionHistory: createFakePositionHistory({
                pos: [
                    createFakePositionProps({
                        san: "e4",
                        sideToMove: GameColor.BLACK,
                    }),
                    createFakePositionProps({
                        san: "e5",
                        sideToMove: GameColor.WHITE,
                    }),
                    createFakePositionProps({
                        san: "Nf3",
                        sideToMove: GameColor.BLACK,
                    }),
                    createFakePositionProps({
                        san: "Nf6",
                        sideToMove: GameColor.WHITE,
                    }),
                ],
            }),
        });

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryRows />
            </ChessboardStoreContext.Provider>,
        );

        const rows = screen.getAllByTestId("moveRow");
        expect(rows).toHaveLength(2);

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
                hasMoved: true,
            }),
            move: createFakeMove({
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 1, y: 0 }),
            }),
            sideToMove: GameColor.BLACK,
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
            sideToMove: GameColor.WHITE,
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
                <MoveHistoryRows />
            </ChessboardStoreContext.Provider>,
        );

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
});
