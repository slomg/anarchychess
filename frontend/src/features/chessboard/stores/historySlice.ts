import { StateCreator } from "zustand";

import { ChessboardStore } from "./chessboardStore";
import { Move, MoveBounds, PieceID, Position } from "../lib/types";
import BoardPieces from "../lib/boardPieces";
import { pointEquals } from "@/features/point/pointUtils";
import LegalMoves from "../lib/legalMoves";

export interface HistorySliceProps {
    positionHistory?: Position[];
    pieces: BoardPieces;
    legalMoves: LegalMoves;
}

export interface HistorySlice {
    viewingPlyIdx: number;
    positionHistory: Position[];

    teleportToPosition(plyIdx: number): Promise<void>;
    shiftMoveViewBy(amount: number): Promise<void>;
    teleportToLatestPosition(): Promise<void>;

    getLatestPosition(): Position;
    applyHistoryPosition({
        moveFromPreviousViewedPosition,
        position,
    }: {
        moveFromPreviousViewedPosition?: Move;
        position: Position;
    }): Promise<void>;
    addPosition(newPosition: Position): void;
}

export function createHistorySlice(
    initState: HistorySliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    HistorySlice
> {
    function findMovedPiecesBetween(
        oldPieces: BoardPieces,
        newPieces: BoardPieces,
    ): PieceID[] {
        const movedPieceIds: PieceID[] = [];
        for (const newPiece of oldPieces) {
            const piece = newPieces.getById(newPiece.id);
            if (!piece) continue;
            if (!pointEquals(piece.position, newPiece.position))
                movedPieceIds.push(newPiece.id);
        }

        return movedPieceIds;
    }

    return (set, get) => ({
        viewingPlyIdx: initState.positionHistory
            ? initState.positionHistory.length - 1
            : 0,
        positionHistory: initState.positionHistory || [
            { pieces: initState.pieces },
        ],

        async teleportToPosition(plyIdx): Promise<void> {
            const {
                positionHistory,
                viewingPlyIdx,
                applyMoveAnimated,
                applyHistoryPosition,
            } = get();
            if (
                plyIdx < 0 ||
                plyIdx >= positionHistory.length ||
                plyIdx === viewingPlyIdx
            ) {
                return;
            }

            const position = positionHistory[plyIdx];
            const isOneStepForward = plyIdx === viewingPlyIdx + 1;

            set((state) => {
                state.viewingPlyIdx = plyIdx;
            });

            const moveThatProducedPosition = position.move;
            if (isOneStepForward && moveThatProducedPosition) {
                await applyMoveAnimated(moveThatProducedPosition);
                return;
            }

            // the move that should be considered "last" from the perspective of the current viewed position.
            // if moving forward in history (plyIdx > viewingPlyIdx), this is the move that produced the current position (position.move).
            // if moving backward in history (plyIdx < viewingPlyIdx), this is the move in the next position,
            // because that was the move that brought us to the current position from the previous step.
            const moveFromPreviousViewedPosition =
                plyIdx > viewingPlyIdx
                    ? position.move
                    : positionHistory[plyIdx + 1]?.move;
            applyHistoryPosition({
                moveFromPreviousViewedPosition,
                position,
            });
        },

        async shiftMoveViewBy(amount) {
            const { teleportToPosition, viewingPlyIdx } = get();
            await teleportToPosition(viewingPlyIdx + amount);
        },

        async teleportToLatestPosition() {
            const { positionHistory, teleportToPosition } = get();
            const lastIndex = positionHistory.length - 1;
            if (lastIndex < 0) throw new Error("positionHistory is empty");
            await teleportToPosition(lastIndex)!;
        },

        getLatestPosition(): Position {
            const { positionHistory } = get();
            return positionHistory[positionHistory.length - 1];
        },

        async applyHistoryPosition({
            moveFromPreviousViewedPosition,
            position,
        }): Promise<void> {
            const { pieces, playAnimation } = get();

            const movedPieceIds = findMovedPiecesBetween(
                pieces,
                position.pieces,
            );

            set((state) => {
                state.pieces = position.pieces;
                state.selectedPieceId = null;
            });

            const moveBounds: MoveBounds | undefined = position.move
                ? {
                      from: position.move.from,
                      to: position.move.to,
                  }
                : undefined;
            const isCapture = moveFromPreviousViewedPosition
                ? moveFromPreviousViewedPosition.captures.length > 0
                : false;
            const isPromotion = moveFromPreviousViewedPosition
                ? moveFromPreviousViewedPosition.promotesTo !== null
                : false;

            await playAnimation({
                newPieces: position.pieces,
                movedPieceIds,

                moveBounds,
                isCapture,
                isPromotion,
                specialType: moveFromPreviousViewedPosition?.specialType,
            });
        },

        addPosition(newPosition: Position) {
            const { teleportToLatestPosition } = get();

            teleportToLatestPosition();
            set((state) => {
                state.positionHistory.push(newPosition);
                state.viewingPlyIdx = state.positionHistory.length - 1;
            });
        },
    });
}
