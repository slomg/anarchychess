import { StateCreator } from "zustand";
import type { ChessboardStore } from "./chessboardStore";
import {
    AnimationStep,
    MoveAnimation,
    MoveBounds,
    PieceID,
    Piece,
} from "../lib/types";
import { LogicalPoint } from "@/features/point/types";
import BoardPieces from "../lib/boardPieces";
import constants from "@/lib/constants";

export interface AnimationSliceProps {
    lastMove?: MoveBounds;
}

export interface AnimationSlice {
    animatingPieces: BoardPieces | null;
    animatingPieceIds: Set<PieceID>;
    removingPieces: Map<PieceID, Piece>;
    lastMove: MoveBounds | null;

    playAnimationBatch(animation: MoveAnimation): Promise<void>;
    playAnimation(animation: AnimationStep): Promise<void>;
    animatePiece(
        pieceId: PieceID,
        newPosition: LogicalPoint,
        pieces: BoardPieces,
    ): Promise<void>;
    clearAnimation(): void;
    resetLastMove(): void;
}

export function createAnimationSlice(
    initState: AnimationSliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    AnimationSlice
> {
    return (set, get) => {
        let currentAnimationCancelToken: { canceled: boolean } | null = null;

        async function processMoveAnimation(
            animation: MoveAnimation,
            persistent: boolean = false,
        ) {
            const { playAudioForAnimationStep } = get();

            if (currentAnimationCancelToken) {
                currentAnimationCancelToken.canceled = true;
            }

            const cancelToken = { canceled: false };
            currentAnimationCancelToken = cancelToken;

            set((state) => {
                state.removingPieces = animation.removedPieces ?? new Map();
            });

            for (let i = 0; i < animation.steps.length; i++) {
                if (cancelToken.canceled) break;

                const step = animation.steps[i];
                playAudioForAnimationStep(step);
                if (step.initialSpawnPositions) {
                    await spawnPieces(step.initialSpawnPositions);
                }

                set((state) => {
                    state.animatingPieces = step.newPieces;
                    state.lastMove = step.moveBounds ?? null;
                });
                await markPiecesAsAnimating(step.movedPieceIds);

                if (i < animation.steps.length - 1) {
                    await new Promise<void>((resolve) =>
                        setTimeout(
                            () => resolve(),
                            constants.ANIMATION_STEP_DELAY_MS,
                        ),
                    );
                }
            }

            if (!cancelToken.canceled && !persistent) {
                set((state) => {
                    state.animatingPieces = null;
                    state.removingPieces = new Map();
                });
            }

            if (currentAnimationCancelToken === cancelToken) {
                currentAnimationCancelToken = null;
            }
        }

        async function spawnPieces(
            initialSpawnPositions: BoardPieces,
        ): Promise<void> {
            set((state) => {
                state.animatingPieces = initialSpawnPositions;
            });
            await new Promise<void>((resolve) => setTimeout(resolve));
        }

        async function markPiecesAsAnimating(pieceIds: Iterable<PieceID>) {
            set((state) => {
                for (const pieceId of pieceIds)
                    state.animatingPieceIds.add(pieceId);
            });

            await new Promise<void>((resolve) =>
                setTimeout(() => {
                    set((state) => {
                        for (const pieceId of pieceIds)
                            state.animatingPieceIds.delete(pieceId);
                    });
                    resolve();
                }, constants.PIECE_ANIMATION_LENGTH_MS),
            );
        }

        return {
            lastMove: initState.lastMove ?? null,
            animatingPieces: null,
            animatingPieceIds: new Set(),
            removingPieces: new Map(),

            async playAnimationBatch(animation) {
                await processMoveAnimation(animation);
            },

            async playAnimation(animation) {
                await processMoveAnimation({
                    steps: [animation],
                });
            },

            async animatePiece(pieceId, newPosition, pieces) {
                const newPieces = new BoardPieces(pieces);
                const piece = newPieces.getById(pieceId);
                if (!piece) return;

                newPieces.addAt(piece, newPosition);
                await processMoveAnimation(
                    {
                        steps: [
                            {
                                newPieces,
                                movedPieceIds: [pieceId],
                            },
                        ],
                    },
                    true,
                );
            },

            clearAnimation() {
                set((state) => {
                    state.animatingPieces = null;
                });
            },
            resetLastMove() {
                set((state) => {
                    state.lastMove = null;
                });
            },
        };
    };
}
