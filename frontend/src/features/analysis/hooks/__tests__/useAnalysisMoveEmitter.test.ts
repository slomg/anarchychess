import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import {
    AnalysisPosition,
    GameColor,
    getNextAnalysisPosition,
    RootAnalysisPosition,
} from "@/lib/apiClient";
import constants from "@/lib/constants";
import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { renderHook } from "@testing-library/react";
import useAnalysisMoveEmitter from "../useAnalysisMoveEmitter";
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";
import { PositionProps } from "@/features/chessboard/lib/position";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";

vi.mock("@/lib/apiClient/definition");

describe("useAnalysisMoveEmitter", () => {
    let chessboardStore: StoreApi<ChessboardStore>;
    let rootPosition: RootAnalysisPosition;

    const getNextAnalysisPositionMock = vi.mocked(getNextAnalysisPosition);

    beforeEach(() => {
        chessboardStore = createChessboardStore();
        rootPosition = {
            fen: constants.INITIAL_FEN,
            moveOptions: {
                legalMoves: [
                    { fromIdx: 0, toIdx: 1, moveKey: "0" },
                    { fromIdx: 2, toIdx: 3, moveKey: "1" },
                ],
                hasForcedMoves: true,
            },
        };

        getNextAnalysisPositionMock.mockResolvedValue({
            error: undefined,
            data: {
                fen: "someFen",
                san: "e4",
                sideToMove: GameColor.BLACK,
                moveOptions: {
                    legalMoves: [createFakeMovePath()],
                    hasForcedMoves: false,
                },
            },
            response: new Response(),
        });
    });

    it("should call getNextAnalysisPosition with correct parameters when not viewing a position", async () => {
        const move = createFakeMove();

        renderHook(() => useAnalysisMoveEmitter(rootPosition, chessboardStore));

        const { pieceMovementEvent } = chessboardStore.getState();
        await pieceMovementEvent.emit(move);

        expect(getNextAnalysisPositionMock).toHaveBeenCalledWith({
            body: {
                fen: rootPosition.fen,
                movingPlayer: GameColor.WHITE,
                piecePosition: move.from,
                moveKey: move.moveKey,
            },
        });
    });

    it("should call getNextAnalysisPosition with correct parameters when viewing a position", async () => {
        const move = createFakeMove();
        const { addPosition } = chessboardStore.getState();
        const initialPosition = addPosition(createFakePosition());

        renderHook(() => useAnalysisMoveEmitter(rootPosition, chessboardStore));

        await chessboardStore.getState().pieceMovementEvent.emit(move);

        expect(getNextAnalysisPositionMock).toHaveBeenCalledWith({
            body: {
                fen: initialPosition.fen,
                movingPlayer: initialPosition.sideToMove,
                piecePosition: move.from,
                moveKey: move.moveKey,
            },
        });
    });

    it("should add the new position and decoded legal moves to the store after API call", async () => {
        const move = createFakeMove();
        const newAnalysisPosition: AnalysisPosition = {
            fen: "10/10/10/10/10/10/10/10/10/R9",
            san: "e4",
            sideToMove: GameColor.BLACK,
            moveOptions: {
                legalMoves: [createFakeMovePath()],
                hasForcedMoves: true,
            },
        };
        getNextAnalysisPositionMock.mockResolvedValue({
            error: undefined,
            data: newAnalysisPosition,
            response: new Response(),
        });

        mockSequentialUUID();
        renderHook(() => useAnalysisMoveEmitter(rootPosition, chessboardStore));

        const { pieces: prevPieces } = chessboardStore.getState();

        await chessboardStore.getState().pieceMovementEvent.emit(move);

        const { positionHistory, legalMovesByPosition, getLegalMoves } =
            chessboardStore.getState();

        expect(positionHistory.totalPlyCount).toBe(1);
        expect(positionHistory.viewingPosition).toEqual(
            expect.objectContaining<PositionProps>({
                pieces: prevPieces,
                move,
                sideToMove: newAnalysisPosition.sideToMove,
                fen: newAnalysisPosition.fen,
                san: newAnalysisPosition.san,
            }),
        );
        expect(legalMovesByPosition.size).toBe(1);
        expect(getLegalMoves()).toEqual(
            decodeMovePathIntoLegalMoves({
                paths: newAnalysisPosition.moveOptions.legalMoves,
                boardWidth: constants.BOARD_WIDTH,
                hasForcedMoves: newAnalysisPosition.moveOptions.hasForcedMoves,
            }),
        );
    });

    it("should go directly to an existing position without calling the API", async () => {
        const positionHistory = new PositionHistory(createFakeBoardPieces());
        const existingPosition = positionHistory.addNextPosition(
            createFakePositionProps(),
        );
        positionHistory.goToStart();
        chessboardStore.setState({ positionHistory });

        renderHook(() => useAnalysisMoveEmitter(rootPosition, chessboardStore));

        await chessboardStore
            .getState()
            .pieceMovementEvent.emit(existingPosition.move);

        expect(getNextAnalysisPositionMock).not.toHaveBeenCalled();
        expect(chessboardStore.getState().positionHistory.viewingPosition).toBe(
            existingPosition,
        );
    });
});
