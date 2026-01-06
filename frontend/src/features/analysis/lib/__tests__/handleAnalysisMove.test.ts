import { StoreApi } from "zustand";
import { Mock } from "vitest";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";
import {
    addAnalysisMove,
    addSidelineAnalysisMove,
} from "../handleAnalysisMove";
import {
    AnalysisMove,
    AnalysisPosition,
    GameColor,
    getNextAnalysisPosition,
} from "@/lib/apiClient";

import { createFakeAnalysisPosition } from "@/lib/testUtils/fakers/analysisPositionFaker";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import { PositionId, PositionProps } from "@/features/chessboard/lib/position";
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { Move } from "@/features/chessboard/lib/types";
import constants from "@/lib/constants";

vi.mock("@/lib/apiClient/definition");

function expectPositionAndLegalMoves(
    addPositionMove: Mock,
    addLegalMovesMock: Mock,
    move: Move,
    newPositionId: PositionId,
    newAnalysisPosition: AnalysisPosition,
    prevPieces: BoardPieces,
) {
    expect(addPositionMove).toHaveBeenCalledExactlyOnceWith<[PositionProps]>({
        pieces: prevPieces,
        move,
        sideToMove: newAnalysisPosition.sideToMove,
        fen: newAnalysisPosition.fen,
        san: newAnalysisPosition.san,
    });
    expect(addLegalMovesMock).toHaveBeenCalledExactlyOnceWith<
        [LegalMoves, PositionId]
    >(
        decodeMovePathIntoLegalMoves({
            paths: newAnalysisPosition.moveOptions.legalMoves,
            boardWidth: constants.BOARD_WIDTH,
            hasForcedMoves: newAnalysisPosition.moveOptions.hasForcedMoves,
        }),
        newPositionId,
    );
}

describe("addAnalysisMove", () => {
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

        await addAnalysisMove(chessboardStore, rootFen, move);

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

        await addAnalysisMove(chessboardStore, rootFen, move);

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
        const newAnalysisPosition = createFakeAnalysisPosition();
        const newPosition = createFakePosition();
        const addPositionMock = vi.fn();
        const addLegalMovesMock = vi.fn();
        chessboardStore.setState({
            allowHistoryChanges: true,
            addPosition: addPositionMock,
            addLegalMoves: addLegalMovesMock,
        });

        getNextAnalysisPositionMock.mockResolvedValue({
            error: undefined,
            data: newAnalysisPosition,
            response: new Response(),
        });
        addPositionMock.mockReturnValue(newPosition);

        const { pieces: prevPieces } = chessboardStore.getState();
        await addAnalysisMove(chessboardStore, rootFen, move);

        expectPositionAndLegalMoves(
            addPositionMock,
            addLegalMovesMock,
            move,
            newPosition.positionId,
            newAnalysisPosition,
            prevPieces,
        );
    });

    it("should go directly to an existing position without calling the API", async () => {
        const positionHistory = new PositionHistory(createFakeBoardPieces());
        const existingPosition = positionHistory.addNextPosition(
            createFakePositionProps(),
        );
        positionHistory.goToStart();
        chessboardStore.setState({ positionHistory });

        await addAnalysisMove(chessboardStore, rootFen, existingPosition.move);

        expect(getNextAnalysisPositionMock).not.toHaveBeenCalled();
        expect(chessboardStore.getState().positionHistory.viewingPosition).toBe(
            existingPosition,
        );
    });
});

describe("addSidelineAnalysisMove", () => {
    let chessboardStore: StoreApi<ChessboardStore>;

    const getNextAnalysisPositionMock = vi.mocked(getNextAnalysisPosition);

    beforeEach(() => {
        chessboardStore = createChessboardStore();
    });

    it("should add the position as a sideline", async () => {
        const move = createFakeMove();
        const newAnalysisPosition = createFakeAnalysisPosition();
        const newPosition = createFakePosition();
        const addSidelinePositionMock = vi.fn();
        const addLegalMovesMock = vi.fn();
        chessboardStore.setState({
            allowHistoryChanges: true,
            addSidelinePosition: addSidelinePositionMock,
            addLegalMoves: addLegalMovesMock,
        });

        getNextAnalysisPositionMock.mockResolvedValue({
            error: undefined,
            data: newAnalysisPosition,
            response: new Response(),
        });
        addSidelinePositionMock.mockReturnValue(newPosition);

        const { pieces: prevPieces } = chessboardStore.getState();
        await addSidelineAnalysisMove(chessboardStore, "root fen", move);

        expectPositionAndLegalMoves(
            addSidelinePositionMock,
            addLegalMovesMock,
            move,
            newPosition.positionId,
            newAnalysisPosition,
            prevPieces,
        );
    });
});
