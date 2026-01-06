import { StateCreator } from "zustand";

import PositionHistory from "../lib/positionHistory";
import { PositionId } from "../lib/position";
import { Position } from "../lib/position";
import { PositionProps } from "../lib/position";

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

    addPosition(props: PositionProps): Position;
    addSidelinePosition(props: PositionProps): Position;
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

        addPosition(props) {
            let position: Position;
            set((state) => {
                position = state.positionHistory.addNextPosition(props);
            });
            return position!;
        },

        addSidelinePosition(props) {
            let position: Position;
            set((state) => {
                position = state.positionHistory.addNextSidelinePosition(props);
            });
            return position!;
        },
    });
}
