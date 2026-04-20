import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";

import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import { GameColor, PieceType } from "@/lib/apiClient";
import BoardPieces from "../../lib/boardPieces";
import MaterialCount from "../MaterialCount";
import { logicalPoint } from "@/features/point/pointUtils";

describe("MaterialCount", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it.each([
        [GameColor.WHITE, GameColor.BLACK],
        [GameColor.BLACK, GameColor.WHITE],
    ])(
        "should render nothing when material is equal",
        (playerColor, opponentColor) => {
            store.setState({
                pieces: BoardPieces.fromPieces(
                    createFakePiece({
                        type: PieceType.PAWN,
                        color: playerColor,
                    }),
                    createFakePiece({
                        type: PieceType.PAWN,
                        color: opponentColor,
                    }),
                ),
            });

            render(
                <ChessboardStoreContext.Provider value={store}>
                    <MaterialCount playerColor={playerColor} />
                </ChessboardStoreContext.Provider>,
            );

            expect(screen.queryAllByTestId(/materialCount-/).length).toBe(0);
            expect(screen.queryByTestId("materialCountTotalValue")).toBeNull();
        },
    );

    it.each([
        [GameColor.WHITE, GameColor.BLACK],
        [GameColor.BLACK, GameColor.WHITE],
    ])("should render piece difference", (playerColor, opponentColor) => {
        store.setState({
            pieces: BoardPieces.fromPieces(
                createFakePiece({
                    type: PieceType.ROOK,
                    color: playerColor,
                    position: logicalPoint({ x: 4, y: 5 }),
                }),
                createFakePiece({
                    type: PieceType.TRAITOR_ROOK,
                    color: null,
                    position: logicalPoint({ x: 5, y: 5 }),
                }),
                createFakePiece({
                    type: PieceType.PAWN,
                    color: opponentColor,
                    position: logicalPoint({ x: 7, y: 5 }),
                }),
            ),
        });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <MaterialCount playerColor={playerColor} />
            </ChessboardStoreContext.Provider>,
        );

        expect(
            screen.getByTestId(`materialCount-${PieceType.ROOK}`),
        ).toBeInTheDocument();
        expect(
            screen.getByTestId(`materialCount-${PieceType.TRAITOR_ROOK}`),
        ).toBeInTheDocument();

        expect(
            screen.queryByTestId(`materialCount-${PieceType.PAWN}`),
        ).not.toBeInTheDocument();
    });

    it("should render correct number of pieces piece difference", () => {
        store.setState({
            pieces: BoardPieces.fromPieces(
                createFakePiece({
                    type: PieceType.PAWN,
                    color: GameColor.WHITE,
                }),
                createFakePiece({
                    type: PieceType.PAWN,
                    color: GameColor.WHITE,
                }),
            ),
        });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <MaterialCount playerColor={GameColor.WHITE} />
            </ChessboardStoreContext.Provider>,
        );

        const pawns = screen.getAllByTestId(`materialCount-${PieceType.PAWN}`);

        expect(pawns.length).toBe(2);
    });

    it.each([
        [GameColor.WHITE, GameColor.BLACK],
        [GameColor.BLACK, GameColor.WHITE],
    ])(
        "should display total material advantage when positive",
        (playerColor, opponentColor) => {
            store.setState({
                pieces: BoardPieces.fromPieces(
                    createFakePiece({
                        type: PieceType.QUEEN,
                        color: playerColor,
                    }),
                    createFakePiece({
                        type: PieceType.ROOK,
                        color: opponentColor,
                    }),
                ),
            });

            render(
                <ChessboardStoreContext.Provider value={store}>
                    <MaterialCount playerColor={playerColor} />
                </ChessboardStoreContext.Provider>,
            );

            expect(
                screen.getByTestId("materialCountTotalValue"),
            ).toHaveTextContent("+5");
        },
    );

    it.each([
        [GameColor.WHITE, GameColor.BLACK],
        [GameColor.BLACK, GameColor.WHITE],
    ])(
        "should not display total when advantage is negative",
        (playerColor, opponentColor) => {
            store.setState({
                pieces: BoardPieces.fromPieces(
                    createFakePiece({
                        type: PieceType.PAWN,
                        color: playerColor,
                    }),
                    createFakePiece({
                        type: PieceType.ROOK,
                        color: opponentColor,
                    }),
                ),
            });

            render(
                <ChessboardStoreContext.Provider value={store}>
                    <MaterialCount playerColor={playerColor} />
                </ChessboardStoreContext.Provider>,
            );

            expect(screen.queryByTestId("materialCountTotalValue")).toBeNull();
        },
    );
});
