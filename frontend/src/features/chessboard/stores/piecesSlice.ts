import { LogicalPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import { PieceID } from "../lib/types";
import { Move } from "../lib/types";
import type { ChessboardStore } from "./chessboardStore";
import { StateCreator } from "zustand";
import { pointEquals } from "@/features/point/pointUtils";
import {
    simulateMove,
    simulateMoveWithIntermediates,
} from "../lib/simulateMove";
import BoardPieces from "../lib/boardPieces";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import { EventBus } from "@/lib/eventBus";

export interface PieceSliceProps {
    pieces: BoardPieces;
    canDrag: boolean;
}

export interface PiecesSlice {
    pieces: BoardPieces;
    selectedPieceId: PieceID | null;
    canDrag: boolean;
    isProcessingMove: boolean;

    pieceMovementEvent: EventBus<[move: Move], void>;

    selectPiece(pieceId: PieceID): boolean;
    unselectPiece(): void;

    handleMousePieceDrop(args: {
        mousePoint: ScreenPoint;
        isDrag: boolean;
        isDoubleClick: boolean;
    }): Promise<{ success: boolean; needsDoubleClick?: boolean }>;
    applyMoveAnimated(move: Move): Promise<void>;
    applyMoveImmediate(move: Move): Promise<void>;

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
            const { applyMoveImmediate, disableMovement, pieceMovementEvent } =
                get();

            const animationPromise = applyMoveImmediate(move);

            disableMovement();
            await pieceMovementEvent.emit(move);

            await animationPromise;
        }

        function detectNeedsDoubleClick(dest: LogicalPoint): boolean {
            const { selectedPieceId, pieces, getLegalMoves } = get();
            if (!selectedPieceId) return false;

            const piece = pieces.getById(selectedPieceId);
            if (!piece) return false;

            const legalMoves = getLegalMoves();
            return (
                pointEquals(piece.position, dest) &&
                legalMoves.hasMovesFromTo(piece.position, dest)
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

        return {
            ...initState,

            selectedPieceId: null,
            animatingPieces: new Set(),
            isProcessingMove: false,

            pieceMovementEvent: new EventBus(),

            selectPiece(pieceId) {
                const { highlightLegalMoves, pieces, selectedPieceId } = get();
                const piece = pieces.getById(pieceId);
                if (!piece) {
                    console.warn(
                        `Cannot show legal moves, no piece was found with id ${pieceId}`,
                    );
                    return false;
                }
                if (pieceId === selectedPieceId) return false;

                const hasLegalMoves = highlightLegalMoves(piece);
                set((state) => {
                    state.selectedPieceId = hasLegalMoves ? pieceId : null;
                });

                return hasLegalMoves;
            },
            unselectPiece() {
                const { hideLegalMoves } = get();

                hideLegalMoves();
                set((state) => {
                    state.selectedPieceId = null;
                });
            },

            async applyMoveImmediate(move: Move): Promise<void> {
                const { playAnimation, pieces } = get();
                const animation = simulateMove(pieces, move);

                set((state) => {
                    state.pieces = animation.newPieces;
                });
                await playAnimation(animation);
            },

            async applyMoveAnimated(move: Move): Promise<void> {
                const { playAnimationBatch, pieces } = get();

                const positions = simulateMoveWithIntermediates(pieces, move);
                const lastPosition = positions.steps.at(-1);
                if (!lastPosition) return;

                set((state) => {
                    state.pieces = lastPosition.newPieces;
                });
                await playAnimationBatch(positions);
            },

            async handleMousePieceDrop({ mousePoint, isDrag, isDoubleClick }) {
                const {
                    screenToLogicalPoint,
                    flashLegalMoves,
                    clearAnimation,
                    getLegalMoves,
                    isProcessingMove,
                } = get();
                if (isProcessingMove) return { success: false };

                set((state) => {
                    state.isProcessingMove = true;
                });
                try {
                    const dest = screenToLogicalPoint(mousePoint);
                    if (!dest) return { success: false };

                    const needsDoubleClick = detectNeedsDoubleClick(dest);
                    if (needsDoubleClick && !isDoubleClick)
                        return { success: false, needsDoubleClick: true };

                    const move = await getMoveForSelection(dest);
                    if (move) {
                        await applyMoveTurn(move);
                        return { success: true };
                    }
                    clearAnimation();

                    const legalMoves = getLegalMoves();
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
                set((state) => {
                    state.pieces = pieces;
                });
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
