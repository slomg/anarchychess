import { StateCreator } from "zustand";

import PositionHistory, {
    Position,
    PositionId,
    PositionProps,
} from "../lib/positionHistory";

import { ChessboardStore } from "./chessboardStore";
import BoardPieces from "../lib/boardPieces";

export interface HistorySliceProps {
    positionHistory?: PositionHistory;
    pieces: BoardPieces;
}

export interface HistorySlice {
    positionHistory: PositionHistory;

    goToPosition(positionId: PositionId): Promise<void>;

    stepPositionForward(): Promise<void>;
    stepPositionBackward(): Promise<void>;
    goToStartPosition(): Promise<void>;
    goToLatestPosition(): Promise<void>;

    addPosition(newPosition: PositionProps): Position;
}

export function createHistorySlice(
    initState: HistorySliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    HistorySlice
> {
    return (set, get) => ({
        positionHistory:
            initState.positionHistory ?? new PositionHistory(initState.pieces),

        async goToPosition(positionId) {
            const { applyMoveAnimated, updatePiecesFromPosition } = get();

            let success: boolean | undefined;
            let isOneStepForward: boolean | undefined;
            set((state) => {
                ({ success, isOneStepForward } =
                    state.positionHistory.goToPosition(positionId));
            });
            const { positionHistory } = get();
            if (!success || !positionHistory.viewingPosition) return;

            if (isOneStepForward) {
                await applyMoveAnimated(positionHistory.viewingPosition.move);
                return;
            }

            await updatePiecesFromPosition(positionHistory.viewingPosition);
        },

        async stepPositionForward() {
            const { applyMoveAnimated } = get();

            let success: boolean | undefined;
            set((state) => {
                success = state.positionHistory.stepForward();
            });

            const { positionHistory } = get();
            if (success && positionHistory.viewingPosition) {
                await applyMoveAnimated(positionHistory.viewingPosition.move);
            }
        },

        async stepPositionBackward() {
            const { updatePiecesFromPosition, updatePieces } = get();

            let success: boolean | undefined;
            set((state) => {
                success = state.positionHistory.stepBackward();
            });
            if (!success) return;

            const { positionHistory } = get();
            if (positionHistory.viewingPosition) {
                await updatePiecesFromPosition(positionHistory.viewingPosition);
            } else {
                await updatePieces(positionHistory.rootPieces);
            }
        },

        async goToStartPosition() {
            const { positionHistory, updatePieces } = get();

            let success: boolean | undefined;
            set((state) => {
                success = state.positionHistory.goToStart();
            });
            if (success) {
                await updatePieces(positionHistory.rootPieces);
            }
        },

        async goToLatestPosition() {
            const { updatePiecesFromPosition, applyMoveAnimated } = get();

            let success: boolean | undefined;
            let isOneStepForward: boolean | undefined;
            set((state) => {
                ({ success, isOneStepForward } =
                    state.positionHistory.goToEnd());
            });

            const { positionHistory } = get();
            if (!success || !positionHistory.viewingPosition) return;

            if (isOneStepForward) {
                await applyMoveAnimated(positionHistory.viewingPosition.move);
            } else {
                await updatePiecesFromPosition(positionHistory.viewingPosition);
            }
        },

        addPosition(newPosition) {
            let position: Position;
            set((state) => {
                position = state.positionHistory.addNextPosition(newPosition);
            });
            return position!;
        },
    });
}
