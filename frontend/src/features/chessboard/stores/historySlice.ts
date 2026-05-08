import { StateCreator } from "zustand";

import PositionHistory from "../lib/positionHistory";
import { ChessboardStore } from "./chessboardStore";
import { PositionProps } from "../lib/position";
import { PositionId } from "../lib/position";
import BoardPieces from "../lib/boardPieces";
import { Position } from "../lib/position";
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

    setPosition(positionId: PositionId): void;

    goToPosition(positionId: PositionId): Promise<void>;
    stepPositionForward(): Promise<void>;
    stepPositionBackward(): Promise<void>;
    goToStartPosition(): Promise<void>;
    goToLatestPosition(): Promise<void>;

    addPosition(props: PositionProps, legalMoves?: LegalMoves): Position;
    addLatestPosition(props: PositionProps, legalMoves?: LegalMoves): Position;
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
    overrideRoot(pieces: BoardPieces): void;

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

        setPosition(positionId) {
            set((state) => {
                state.positionHistory.goToPosition(positionId);
            });
        },

        async goToPosition(positionId) {
            const { applyMoveAnimated, updatePieces } = get();

            let position!: Position | null;
            let success!: boolean;
            let isOneStepForward!: boolean;
            set((state) => {
                ({ success, isOneStepForward } =
                    state.positionHistory.goToPosition(positionId));
                position = state.positionHistory.viewingPosition;
            });

            if (!success || !position) {
                return;
            }

            if (isOneStepForward) {
                await applyMoveAnimated(position.move);
            } else {
                await updatePieces(position.pieces);
            }
        },

        async stepPositionForward() {
            const { applyMoveAnimated } = get();

            let success!: boolean;
            let position!: Position | null;
            set((state) => {
                success = state.positionHistory.stepForward();
                position = state.positionHistory.viewingPosition;
            });

            if (success && position) {
                await applyMoveAnimated(position.move);
            }
        },

        async stepPositionBackward() {
            const { updatePieces } = get();

            let success!: boolean;
            let position!: Position | null;
            set((state) => {
                success = state.positionHistory.stepBackward();
                position = state.positionHistory.viewingPosition;
            });
            if (!success) {
                return;
            }

            const { positionHistory } = get();
            if (position) {
                await updatePieces(position.pieces);
            } else {
                await updatePieces(positionHistory.root.pieces);
            }
        },

        async goToStartPosition() {
            const { positionHistory, updatePieces } = get();

            let success!: boolean;
            set((state) => {
                success = state.positionHistory.goToStart();
            });
            if (success) {
                await updatePieces(positionHistory.root.pieces);
            }
        },

        async goToLatestPosition() {
            const { updatePieces, applyMoveAnimated } = get();

            let position!: Position | null;
            let success!: boolean;
            let isOneStepForward!: boolean;
            set((state) => {
                ({ success, isOneStepForward } =
                    state.positionHistory.goToEnd());
                position = state.positionHistory.viewingPosition;
            });

            if (!success || !position) {
                return;
            }

            if (isOneStepForward) {
                await applyMoveAnimated(position.move);
            } else {
                await updatePieces(position.pieces);
            }
        },

        addPosition(props, legalMoves) {
            let position!: Position;
            set((state) => {
                position = state.positionHistory.addNextPosition(props);
                if (legalMoves) {
                    state.legalMovesByPosition.set(
                        position.positionId,
                        legalMoves,
                    );
                }
            });

            return position;
        },

        addLatestPosition(props, legalMoves) {
            let position!: Position;
            set((state) => {
                state.positionHistory.goToEnd();
                position = state.positionHistory.addNextPosition(props);
                if (legalMoves) {
                    state.legalMovesByPosition.set(
                        position.positionId,
                        legalMoves,
                    );
                }
            });
            return position;
        },

        addSidelinePosition(props, legalMoves) {
            let position!: Position;
            set((state) => {
                position = state.positionHistory.addNextSidelinePosition(props);
                if (legalMoves) {
                    state.legalMovesByPosition.set(
                        position.positionId,
                        legalMoves,
                    );
                }
            });

            return position;
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
                return LegalMoves.StableEmpty;
            }

            return (
                legalMovesByPosition.get(
                    positionHistory.viewingPosition?.positionId,
                ) ?? LegalMoves.StableEmpty
            );
        },

        addLegalMovesForPosition(legalMoves, positionId) {
            set((state) => {
                state.legalMovesByPosition.set(positionId, legalMoves);
            });
        },
        setLatestLegalMoves(legalMoves) {
            const { positionHistory, addLegalMovesForPosition } = get();

            addLegalMovesForPosition(
                legalMoves,
                positionHistory.viewingPosition?.positionId,
            );
        },

        overrideRoot(pieces) {
            const { setImmediatePieces } = get();
            set((state) => {
                state.positionHistory.overrideRoot(pieces);
            });
            setImmediatePieces(pieces);
        },

        setAllowHistoryChanges(value) {
            set((state) => {
                state.allowHistoryChanges = value;
            });
        },
    });
}
