import { StateCreator } from "zustand";

import {
    simulateMove,
    simulateMoveWithIntermediates,
} from "../lib/simulateMove";

import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import { pointEquals, pointToStr } from "@/features/point/pointUtils";
import { AnimationStep, MoveBounds, PieceID } from "../lib/types";
import type { ChessboardStore } from "./chessboardStore";
import { LogicalPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import BoardPieces from "../lib/boardPieces";
import { Position } from "../lib/position";
import EventBus from "@/lib/eventBus";
import { Move } from "../lib/types";

export interface PieceSliceProps {
    pieces: BoardPieces;
    disableDrag?: boolean;
}

export interface PiecesSlice {
    pieces: BoardPieces;
    selectedPieceId: PieceID | null;
    disableDrag: boolean;
    isProcessingMove: boolean;

    pieceMovementEvent: EventBus<[move: Move, prevPieces: BoardPieces], void>;

    selectPiece(pieceId: PieceID): boolean;
    unselectPiece(): void;

    applyMoveAnimated(move: Move): Promise<void>;
    applyMoveImmediate(move: Move): Promise<void>;
    removePieceAt(point: LogicalPoint): Promise<void>;

    handleMousePieceDrop(args: {
        mousePoint: ScreenPoint;
        isDrag: boolean;
        isDoubleClick: boolean;
    }): Promise<{ success: boolean; needsDoubleClick?: boolean }>;

    updatePiecesFromPosition(position: Position): Promise<void>;
    updatePieces(newPieces: BoardPieces): Promise<void>;
    setImmediatePieces(pieces: BoardPieces): void;

    screenPointToPiece(position: ScreenPoint): PieceID | undefined;
}

export function createPiecesSlice(
    initState: PieceSliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    PiecesSlice
> {
    return (set, get) => {
        async function applyMoveTurn(move: Move): Promise<void> {
            const {
                applyMoveImmediate,
                unselectPiece,
                pieces: prevPieces,
                pieceMovementEvent,
            } = get();

            const animationPromise = applyMoveImmediate(move);

            unselectPiece();
            await pieceMovementEvent.emit(move, prevPieces);

            await animationPromise;
        }

        function detectNeedsDoubleClick(dest: LogicalPoint): boolean {
            const { selectedPieceId, pieces, getViewedPositionLegalMoves } =
                get();
            if (!selectedPieceId) return false;

            const piece = pieces.getById(selectedPieceId);
            if (!piece) return false;

            const legalMoves = getViewedPositionLegalMoves();
            return (
                pointEquals(piece.position, dest) &&
                legalMoves.hasMovesDirectlyFromTo(piece.position, dest)
            );
        }

        async function getMoveForSelection(
            dest: LogicalPoint,
        ): Promise<Move | null> {
            const { selectedPieceId, getLegalMove, pieces } = get();
            if (!selectedPieceId) return null;

            const move = await getLegalMove(dest, selectedPieceId, pieces);
            return move;
        }

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

        function commitPositionChange(newPieces: BoardPieces) {
            const { discardAllPrompts } = get();

            discardAllPrompts();
            set((state) => {
                state.pieces = newPieces;
            });
        }

        return {
            ...initState,

            disableDrag: initState.disableDrag ?? false,

            selectedPieceId: null,
            animatingPieces: new Set(),
            isProcessingMove: false,

            pieceMovementEvent: new EventBus(),

            selectPiece(pieceId) {
                const { pieces, selectedPieceId } = get();
                const piece = pieces.getById(pieceId);
                if (!piece) {
                    console.warn(
                        `Cannot show legal moves, no piece was found with id ${pieceId}`,
                    );
                    return false;
                }
                if (pieceId === selectedPieceId) return false;

                set((state) => {
                    state.selectedPieceId = pieceId;
                });
                return true;
            },
            unselectPiece() {
                set((state) => {
                    state.selectedPieceId = null;
                });
            },

            async applyMoveImmediate(move) {
                const { playAnimation, pieces } = get();
                const animation = simulateMove(pieces, move);

                commitPositionChange(animation.newPieces);
                await playAnimation(animation);
            },

            async applyMoveAnimated(move) {
                const { playAnimation, pieces } = get();

                const steps = simulateMoveWithIntermediates(pieces, move);
                const lastStep = steps.at(-1);
                if (!lastStep) return;

                commitPositionChange(lastStep.newPieces);
                await playAnimation(steps);
            },

            async removePieceAt(point) {
                const { pieces, discardPromptsForPiece, playAnimation } = get();

                const removePiece = pieces.getByPosition(point);
                if (!removePiece) {
                    console.warn(
                        `Could not find piece to remove at ${pointToStr(point)}`,
                    );
                    return;
                }
                const newPieces = new BoardPieces(pieces);
                newPieces.remove(removePiece.id);

                const animation: AnimationStep = {
                    newPieces,
                    movedPieceIds: [],
                    fadedPieces: new Map([[removePiece.id, removePiece]]),
                };

                discardPromptsForPiece(removePiece.id);
                set((state) => {
                    state.pieces = newPieces;
                });
                await playAnimation(animation);
            },

            async handleMousePieceDrop({ mousePoint, isDrag, isDoubleClick }) {
                const {
                    screenToLogicalPoint,
                    flashLegalMoves,
                    clearAnimation,
                    getViewedPositionLegalMoves,
                    isProcessingMove,
                } = get();
                if (isProcessingMove) {
                    return { success: false };
                }

                set((state) => {
                    state.isProcessingMove = true;
                });
                try {
                    const dest = screenToLogicalPoint(mousePoint);
                    if (!dest) {
                        return { success: false };
                    }

                    const needsDoubleClick = detectNeedsDoubleClick(dest);
                    if (needsDoubleClick && !isDoubleClick) {
                        return { success: false, needsDoubleClick: true };
                    }

                    const move = await getMoveForSelection(dest);
                    if (move) {
                        await applyMoveTurn(move);
                        return { success: true };
                    }
                    clearAnimation();

                    const legalMoves = getViewedPositionLegalMoves();
                    if (
                        legalMoves.hasForcedMoves &&
                        isDrag // player tried to phyically move the piece, not just click and click somewhere else
                    ) {
                        flashLegalMoves();
                        AudioPlayer.playAudio(AudioType.ILLEGAL_MOVE);
                    }

                    return { success: false };
                } finally {
                    set((state) => {
                        state.isProcessingMove = false;
                    });
                }
            },

            setImmediatePieces(pieces) {
                const { resetLastMove } = get();
                resetLastMove();
                commitPositionChange(pieces);
            },

            async updatePiecesFromPosition(position) {
                const { pieces, playAnimation } = get();

                const movedPieceIds = findMovedPiecesBetween(
                    pieces,
                    position.pieces,
                );

                const moveBounds: MoveBounds = {
                    from: position.move.from,
                    to: position.move.to,
                };
                const isCapture = position.move.captures.length > 0;
                const isPromotion = position.move.promotesTo !== null;

                commitPositionChange(position.pieces);
                await playAnimation({
                    newPieces: position.pieces,
                    movedPieceIds,

                    moveBounds,
                    isCapture,
                    isPromotion,
                    specialType: position.move.specialType,
                });
            },
            async updatePieces(newPieces) {
                const { pieces, playAnimation } = get();

                const movedPieceIds = findMovedPiecesBetween(pieces, newPieces);
                commitPositionChange(newPieces);
                await playAnimation({ newPieces, movedPieceIds });
            },

            screenPointToPiece(point) {
                const { screenToLogicalPoint, pieces } = get();

                const logicalPoint = screenToLogicalPoint(point);
                if (!logicalPoint) return;

                return pieces.getByPosition(logicalPoint)?.id;
            },
        };
    };
}
