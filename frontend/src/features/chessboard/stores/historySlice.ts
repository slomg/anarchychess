import { StateCreator } from "zustand";

import PositionHistory from "../lib/positionHistory";
import { PositionId } from "../lib/position";
import { Position } from "../lib/position";
import { PositionProps } from "../lib/position";

import { ChessboardStore } from "./chessboardStore";
import BoardPieces from "../lib/boardPieces";
import LegalMoves from "../lib/legalMoves";

export interface HistorySliceProps {
    legalMovesByPosition: Map<PositionId | undefined, LegalMoves>;
    positionHistory?: PositionHistory;
    allowHistoryChanges?: boolean;
    pieces: BoardPieces;
}

export interface HistorySlice {
    positionHistory: PositionHistory;
    legalMovesByPosition: Map<PositionId | undefined, LegalMoves>;
    allowHistoryChanges: boolean;

    goToPosition(positionId: PositionId): Promise<void>;
    stepPositionForward(): Promise<void>;
    stepPositionBackward(): Promise<void>;
    goToStartPosition(): Promise<void>;
    goToLatestPosition(): Promise<void>;

    addPosition(props: PositionProps, legalMoves?: LegalMoves): Position;
    addSidelinePosition(
        props: PositionProps,
        legalMoves?: LegalMoves,
    ): Position;

    getViewedPositionLegalMoves(): LegalMoves;
    addLegalMovesForPosition(
        legalMoves: LegalMoves,
        positionId?: PositionId,
    ): void;
    setLatestLegalMoves(legalMoves: LegalMoves): void;

    setAllowHistoryChanges(value: boolean): void;
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
        legalMovesByPosition: initState.legalMovesByPosition,
        allowHistoryChanges: initState.allowHistoryChanges ?? false,
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

        addPosition(props, legalMoves) {
            const { unhighlightLegalMoves, unselectPiece } = get();

            let position: Position;
            set((state) => {
                position = state.positionHistory.addNextPosition(props);

                if (legalMoves) {
                    state.legalMovesByPosition.set(
                        position.positionId,
                        legalMoves,
                    );
                }
            });
            unhighlightLegalMoves();
            unselectPiece();

            return position!;
        },

        addSidelinePosition(props, legalMoves) {
            const { unselectPiece, unhighlightLegalMoves } = get();

            let position: Position;
            set((state) => {
                position = state.positionHistory.addNextSidelinePosition(props);

                if (legalMoves) {
                    state.legalMovesByPosition.set(
                        position.positionId,
                        legalMoves,
                    );
                }
            });
            unhighlightLegalMoves();
            unselectPiece();

            return position!;
        },

        getViewedPositionLegalMoves() {
            const {
                legalMovesByPosition,
                allowHistoryChanges,
                positionHistory,
                hideLegalMoves,
            } = get();

            const cannotModifyHistory =
                !allowHistoryChanges &&
                !positionHistory.isViewingLatestPosition;
            if (hideLegalMoves || cannotModifyHistory) {
                return new LegalMoves();
            }

            return (
                legalMovesByPosition.get(
                    positionHistory.viewingPosition?.positionId,
                ) ?? new LegalMoves()
            );
        },

        addLegalMovesForPosition(legalMoves, positionId) {
            const { unhighlightLegalMoves, unselectPiece } = get();
            set((state) => {
                state.legalMovesByPosition.set(positionId, legalMoves);
            });
            unhighlightLegalMoves();
            unselectPiece();
        },
        setLatestLegalMoves(legalMoves) {
            const { positionHistory, addLegalMovesForPosition } = get();

            addLegalMovesForPosition(
                legalMoves,
                positionHistory.viewingPosition?.positionId,
            );
        },

        setAllowHistoryChanges(value) {
            set((state) => {
                state.allowHistoryChanges = value;
            });
        },
    });
}
