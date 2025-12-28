import { StateCreator } from "zustand";

import { ChessboardStore } from "./chessboardStore";
import {
    MoveBounds,
    PieceID,
    Position,
    ProcessedMoveOptions,
} from "../lib/types";
import BoardPieces from "../lib/boardPieces";
import { pointEquals } from "@/features/point/pointUtils";

export interface HistorySliceProps {
    positionHistory?: Position[];
    pieces: BoardPieces;
    moveOptions: ProcessedMoveOptions;
}

export interface HistorySlice {
    viewingPlyIdx: number;
    positionHistory: Position[];

    teleportToMove(plyIdx: number): Promise<void>;
    shiftMoveViewBy(amount: number): Promise<void>;
    teleportToLatestMove(): Promise<void>;

    getLatestPosition(): Position;
    receivePosition(newPosition: Position): void;
}

export function createHistorySlice(
    initState: HistorySliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    HistorySlice
> {
    return (set, get) => {
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

        async function applyHistoryPosition(
            newPlyIdx: number,
            viewingPlyIdx: number,
            position: Position,
        ): Promise<void> {
            const { pieces, positionHistory, playAnimation } = get();

            const movedPieceIds = findMovedPiecesBetween(
                pieces,
                position.pieces,
            );

            set((state) => {
                state.pieces = position.pieces;
                state.selectedPieceId = null;
            });

            // the move that should be considered "last" from the perspective of the current viewed position.
            // if moving forward in history (number > viewingMoveNumber), this is the move that produced the current position (position.move).
            // if moving backward in history (number < viewingMoveNumber), this is the move in the next position,
            // because that was the move that brought us to the current position from the previous step.
            const moveFromPreviousViewedPosition =
                newPlyIdx > viewingPlyIdx
                    ? position.move
                    : positionHistory[newPlyIdx + 1]?.move;

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
                specialType: moveFromPreviousViewedPosition?.specialMoveType,
            });
        }

        return {
            viewingPlyIdx: initState.positionHistory
                ? initState.positionHistory.length - 1
                : 0,
            positionHistory: initState.positionHistory || [
                {
                    pieces: initState.pieces,
                    moveOptions: initState.moveOptions,
                },
            ],

            async teleportToMove(plyIdx): Promise<void> {
                const { positionHistory, viewingPlyIdx, applyMoveAnimated } =
                    get();
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

                await applyHistoryPosition(plyIdx, viewingPlyIdx, position);
            },

            async shiftMoveViewBy(amount) {
                const { teleportToMove, viewingPlyIdx } = get();
                await teleportToMove(viewingPlyIdx + amount);
            },

            async teleportToLatestMove() {
                const { positionHistory, teleportToMove } = get();
                const lastIndex = positionHistory.length - 1;
                if (lastIndex < 0) throw new Error("positionHistory is empty");
                await teleportToMove(lastIndex)!;
            },

            getLatestPosition(): Position {
                const { positionHistory } = get();
                return positionHistory[positionHistory.length - 1];
            },

            receivePosition(newPosition: Position) {
                set((state) => {
                    state.positionHistory.push(newPosition);
                    state.viewingPlyIdx = state.positionHistory.length - 1;
                });
            },
        };
    };
}
