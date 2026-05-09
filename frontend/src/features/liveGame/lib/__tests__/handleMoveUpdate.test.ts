import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createNFakePositionHistory } from "@/lib/testUtils/fakers/positionHistoryFaker";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";
import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import { Clocks, GameColor, MoveSnapshot } from "@/lib/apiClient";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { idxToLogicalPoint, logicalPoint } from "@/features/point/pointUtils";
import handleMoveUpdate from "../handleMoveUpdate";
import { decodeMovePath } from "../moveDecoder";

describe("handleMoveUpdate", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();
    });

    async function triggerMoveMade({
        sideToMove,
        clocks,
        legalMoves,
        plyNumber,
    }: {
        sideToMove: GameColor;
        clocks?: Clocks;
        legalMoves?: LegalMoves;
        plyNumber?: number;
    }): Promise<MoveSnapshot> {
        const move = createFakeMoveSnapshot({
            san: "test san",
            path: { fromIdx: 11, toIdx: 12, moveKey: "0" },
            nextSideToMove: sideToMove,
        });
        plyNumber ??=
            chessboardStore.getState().positionHistory.mainPlyCount + 1;

        await handleMoveUpdate(liveChessStore, chessboardStore, {
            move,
            plyNumber,
            legalMoves,
            clocks,
        });

        return move;
    }

    function setupStoresForMove(isPendingMoveAck: boolean = false) {
        const piece = createFakePiece();

        // board is after the animation, history still hasn't received animation yet
        const boardPieces = BoardPieces.fromPieces({
            ...piece,
            position: isPendingMoveAck
                ? logicalPoint({ x: 2, y: 1 })
                : logicalPoint({ x: 1, y: 1 }),
            hasMoved: isPendingMoveAck,
        });

        const historyPieces = BoardPieces.fromPieces({
            ...piece,
            position: logicalPoint({ x: 1, y: 1 }),
        });

        const positionHistory = createNFakePositionHistory(3);
        positionHistory.addNextPosition(
            createFakePositionProps({ pieces: historyPieces }),
        );

        chessboardStore.setState({ positionHistory, pieces: boardPieces });
        liveChessStore.setState({
            viewer: { userId: "test id", playerColor: GameColor.WHITE },
        });
        return piece;
    }

    it("should return false if the ply number is not the next expected ply", async () => {
        chessboardStore.setState({
            positionHistory: createNFakePositionHistory(1),
        });

        await triggerMoveMade({
            sideToMove: GameColor.BLACK,
            plyNumber: 23,
        });

        expect(chessboardStore.getState().positionHistory.totalPlyCount).toBe(
            1,
        );
    });

    it.each([true, false])(
        "should only play and store the move if we are not awaiting move ack",
        async (awaitingAck) => {
            setupStoresForMove(awaitingAck);

            const {
                pieces: piecesBefore,
                positionHistory: positionHistoryBefore,
            } = chessboardStore.getState();

            if (awaitingAck) {
                liveChessStore.getState().markPendingMoveAck();
            }

            const move = await triggerMoveMade({
                sideToMove: GameColor.BLACK,
            });

            expect(
                chessboardStore.getState().positionHistory.mainPlyCount,
            ).toBe(positionHistoryBefore.mainPlyCount + 1);

            const piecesAfter = chessboardStore.getState().pieces;
            if (awaitingAck) {
                expect(piecesAfter).toEqual(piecesBefore);
            } else {
                expect(piecesAfter).not.toEqual(piecesBefore);
            }

            expect(
                chessboardStore.getState().positionHistory.viewingPosition,
            ).toEqual(
                expect.objectContaining({
                    san: move.san,
                    move: decodeMovePath(move.path),
                    pieces: piecesAfter,
                }),
            );
        },
    );

    it("should go to the last position before playing the move", async () => {
        setupStoresForMove();

        const { goToStartPosition } = chessboardStore.getState();
        await goToStartPosition();

        await triggerMoveMade({
            sideToMove: GameColor.BLACK,
        });

        const {
            positionHistory: updatedPositionHistory,
            pieces: updatedPieces,
        } = chessboardStore.getState();

        expect(updatedPositionHistory.isViewingLatestPosition).toBe(true);
        expect(updatedPositionHistory.viewingPosition?.pieces).toEqual(
            updatedPieces,
        );
    });
});
