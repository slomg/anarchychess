import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";
import {
    AnalysisMove,
    AnalysisPosition,
    GameColor,
    getNextAnalysisPosition,
} from "@/lib/apiClient";

import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { PositionProps } from "@/features/chessboard/lib/position";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import handleAnalysisMove from "../handleAnalysisMove";
import constants from "@/lib/constants";

vi.mock("@/lib/apiClient/definition");

describe("handleAnalysisMove", () => {
    let chessboardStore: StoreApi<ChessboardStore>;
    const rootFen = "test root fen";

    const getNextAnalysisPositionMock = vi.mocked(getNextAnalysisPosition);

    beforeEach(() => {
        chessboardStore = createChessboardStore();

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

    it("should call getNextAnalysisPosition with the root fen when not viewing a position", async () => {
        const move = createFakeMove();

        await handleAnalysisMove(chessboardStore, rootFen, move);

        expect(getNextAnalysisPositionMock).toHaveBeenCalledWith<
            [{ body: AnalysisMove }]
        >({
            body: {
                fen: rootFen,
                piecePosition: move.from,
                moveKey: move.moveKey,
            },
        });
    });

    it("should call getNextAnalysisPosition viewing position fen when viewing a position", async () => {
        const move = createFakeMove();
        const { addPosition } = chessboardStore.getState();
        const initialPosition = addPosition(createFakePosition());

        await handleAnalysisMove(chessboardStore, rootFen, move);

        expect(getNextAnalysisPositionMock).toHaveBeenCalledWith<
            [{ body: AnalysisMove }]
        >({
            body: {
                fen: initialPosition.fen,
                piecePosition: move.from,
                moveKey: move.moveKey,
            },
        });
    });

    it("should add the new position and decoded legal moves to the store", async () => {
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

        const { pieces: prevPieces } = chessboardStore.getState();
        await handleAnalysisMove(chessboardStore, rootFen, move);

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

        await handleAnalysisMove(
            chessboardStore,
            rootFen,
            existingPosition.move,
        );

        expect(getNextAnalysisPositionMock).not.toHaveBeenCalled();
        expect(chessboardStore.getState().positionHistory.viewingPosition).toBe(
            existingPosition,
        );
    });
});
