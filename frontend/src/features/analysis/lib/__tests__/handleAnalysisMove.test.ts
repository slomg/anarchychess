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
    ChildPositionNode,
    PositionProps,
} from "@/features/chessboard/lib/position";
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
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { Move } from "@/features/chessboard/lib/types";

vi.mock("@/lib/apiClient/definition");

function expectPositionAndLegalMoves(
    addPositionMove: Mock,
    move: Move,
    newAnalysisPosition: AnalysisPosition,
    newPieces: BoardPieces,
) {
    expect(addPositionMove).toHaveBeenCalledExactlyOnceWith<
        [PositionProps, LegalMoves]
    >(
        {
            pieces: newPieces,
            move,
            sideToMove: newAnalysisPosition.sideToMove,
            fen: newAnalysisPosition.fen,
            san: newAnalysisPosition.san,
        },
        decodeMovePathIntoLegalMoves(newAnalysisPosition.legalMoves),
    );
}

describe("addAnalysisMove", () => {
    let chessboardStore: StoreApi<ChessboardStore>;
    const rootFen = "test root fen";

    let newPieces: BoardPieces;
    let prevPieces: BoardPieces;
    let move: Move;

    const getNextAnalysisPositionMock = vi.mocked(getNextAnalysisPosition);

    beforeEach(() => {
        chessboardStore = createChessboardStore();

        newPieces = createFakeBoardPieces(1);
        prevPieces = createFakeBoardPieces(2);
        move = createFakeMove();

        // set pieces to something else, like a move was just played and changed from prevPieces to that
        chessboardStore.setState({
            pieces: newPieces,
            allowHistoryChanges: true,
            hideLegalMoves: false,
        });

        getNextAnalysisPositionMock.mockResolvedValue({
            error: undefined,
            data: {
                fen: "someFen",
                san: "e4",
                sideToMove: GameColor.BLACK,
                legalMoves: [createFakeMovePath()],
            },
            response: new Response(),
        });
    });

    it("should call getNextAnalysisPosition with the root fen when not viewing a position", async () => {
        await addAnalysisMove({ chessboardStore, rootFen, move, prevPieces });

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
        const { addPosition } = chessboardStore.getState();
        const initialPosition = addPosition(createFakePositionProps());

        await addAnalysisMove({ chessboardStore, rootFen, move, prevPieces });

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
        const newAnalysisPosition = createFakeAnalysisPosition();
        const newPosition = new ChildPositionNode(createFakePositionProps());
        const addPositionMock = vi.fn();
        chessboardStore.setState({ addPosition: addPositionMock });

        getNextAnalysisPositionMock.mockResolvedValue({
            error: undefined,
            data: newAnalysisPosition,
            response: new Response(),
        });
        addPositionMock.mockReturnValue(newPosition);

        await addAnalysisMove({ chessboardStore, rootFen, move, prevPieces });

        expect(chessboardStore.getState().pieces).toBe(newPieces);
        expectPositionAndLegalMoves(
            addPositionMock,
            move,
            newAnalysisPosition,
            newPieces,
        );
    });

    it("should go directly to an existing position without calling the API", async () => {
        const positionHistory = new PositionHistory(createFakeBoardPieces());
        const existingPosition = positionHistory.addNextPosition(
            createFakePositionProps(),
        );
        positionHistory.goToStart();
        chessboardStore.setState({ positionHistory });

        await addAnalysisMove({
            chessboardStore,
            prevPieces,
            rootFen,
            move: existingPosition.move,
        });

        expect(getNextAnalysisPositionMock).not.toHaveBeenCalled();
        expect(chessboardStore.getState().positionHistory.viewingPosition).toBe(
            existingPosition,
        );
    });

    it("should restore prevPieces if getNextAnalysisPosition throws", async () => {
        const addPositionMock = vi.fn();
        chessboardStore.setState({ addPosition: addPositionMock });
        getNextAnalysisPositionMock.mockImplementationOnce(() => {
            throw new Error("API failure");
        });

        await expect(
            addAnalysisMove({ chessboardStore, rootFen, move, prevPieces }),
        ).rejects.toThrow("API failure");

        const { pieces, hideLegalMoves } = chessboardStore.getState();
        expect(pieces).toEqual(prevPieces);
        expect(hideLegalMoves).toBe(false);
        expect(addPositionMock).not.toHaveBeenCalled();
    });

    it("should restore prevPieces if getNextAnalysisPosition returns an error", async () => {
        const addPositionMock = vi.fn();
        chessboardStore.setState({ addPosition: addPositionMock });
        getNextAnalysisPositionMock.mockResolvedValueOnce({
            error: { errors: [] },
            data: undefined,
            response: new Response(),
        });

        await addAnalysisMove({ chessboardStore, rootFen, move, prevPieces });

        const { pieces, hideLegalMoves } = chessboardStore.getState();
        expect(pieces).toEqual(prevPieces);
        expect(hideLegalMoves).toBe(false);
        expect(addPositionMock).not.toHaveBeenCalled();
    });

    it("should hide legal moves while fetching", async () => {
        chessboardStore.setState({ hideLegalMoves: false });

        const promise = addAnalysisMove({
            chessboardStore,
            rootFen,
            move,
            prevPieces,
        });

        expect(chessboardStore.getState().hideLegalMoves).toBe(true);

        await promise;
        expect(chessboardStore.getState().hideLegalMoves).toBe(false);
    });

    it("should revert hideLegalMoves to what it was previously", async () => {
        chessboardStore.setState({ hideLegalMoves: true });

        await addAnalysisMove({
            chessboardStore,
            rootFen,
            move,
            prevPieces,
        });

        expect(chessboardStore.getState().hideLegalMoves).toBe(true);
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
        const newPosition = new ChildPositionNode(createFakePositionProps());
        const addSidelinePositionMock = vi.fn();
        chessboardStore.setState({
            allowHistoryChanges: true,
            addSidelinePosition: addSidelinePositionMock,
        });

        getNextAnalysisPositionMock.mockResolvedValue({
            error: undefined,
            data: newAnalysisPosition,
            response: new Response(),
        });
        addSidelinePositionMock.mockReturnValue(newPosition);

        const { pieces: newPieces } = chessboardStore.getState();
        await addSidelineAnalysisMove({
            chessboardStore,
            rootFen: "root fen",
            move,
            prevPieces: newPieces,
        });

        expectPositionAndLegalMoves(
            addSidelinePositionMock,
            move,
            newAnalysisPosition,
            newPieces,
        );
    });
});
