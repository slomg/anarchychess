import { StoreApi } from "zustand";

import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import { AnimationStep, MoveBounds } from "../../lib/types";
import { logicalPoint } from "@/features/point/pointUtils";
import BoardPieces from "../../lib/boardPieces";
import constants from "@/lib/constants";
import { createFakePawnThrowEffect } from "@/lib/testUtils/fakers/pawnThrowEffectFaker";
import { BoardEffectId } from "../boardEffectsSlice";

describe("AnimationSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    describe("playAnimation", () => {
        it("should set animatingPieces and animatingPieces during animation and clear after", async () => {
            const piece = createFakePiece();
            const animation: AnimationStep = {
                newPieces: BoardPieces.fromPieces(piece),
                movedPieceIds: [piece.id],
            };

            const promise = store.getState().playAnimation(animation);

            expect(store.getState().animatingPieces).toEqual(
                animation.newPieces,
            );
            expect(store.getState().animatingPieceIds).toEqual(
                new Set([piece.id]),
            );

            vi.advanceTimersByTime(constants.PIECE_ANIMATION_LENGTH_MS);
            await promise;

            expect(store.getState().animatingPieces).toBeNull();
            expect(store.getState().animatingPieceIds.size).toBe(0);
        });

        it("should cancel a previous animation when a new one starts", async () => {
            const piece1 = createFakePiece();
            const piece2 = createFakePiece();
            const firstAnimation: AnimationStep = {
                newPieces: BoardPieces.fromPieces(piece1),
                movedPieceIds: [piece1.id],
            };
            const secondAnimation: AnimationStep = {
                newPieces: BoardPieces.fromPieces(piece2),
                movedPieceIds: [piece2.id],
            };

            const firstPromise = store.getState().playAnimation(firstAnimation);

            vi.advanceTimersByTime(50);

            const secondPromise = store
                .getState()
                .playAnimation(secondAnimation);

            expect(store.getState().animatingPieces).toEqual(
                secondAnimation.newPieces,
            );

            vi.advanceTimersByTime(constants.PIECE_ANIMATION_LENGTH_MS);
            await Promise.all([firstPromise, secondPromise]);

            expect(store.getState().animatingPieces).toBeNull();
            expect(store.getState().animatingPieceIds.size).toBe(0);
        });

        it("should handle fadedPieces correctly", async () => {
            const movingPiece = createFakePiece();
            const removedPiece1 = createFakePiece();
            const removedPiece2 = createFakePiece();
            const animationSteps: AnimationStep[] = [
                {
                    newPieces: BoardPieces.fromPieces(movingPiece),
                    movedPieceIds: [movingPiece.id],
                    isCapture: true,
                    fadedPieces: new Map([[removedPiece1.id, removedPiece1]]),
                },
                {
                    newPieces: BoardPieces.fromPieces(movingPiece),
                    movedPieceIds: [movingPiece.id],
                    isCapture: true,
                    fadedPieces: new Map([[removedPiece2.id, removedPiece2]]),
                },
            ];

            const promise = store.getState().playAnimation(animationSteps);

            expect(store.getState().removingPieces).toEqual(
                animationSteps[0].fadedPieces,
            );

            vi.advanceTimersByTime(constants.PIECE_ANIMATION_LENGTH_MS);
            await flushMicrotasks();
            vi.advanceTimersByTime(constants.ANIMATION_STEP_DELAY_MS);
            await flushMicrotasks();

            expect(store.getState().removingPieces).toEqual(
                animationSteps[1].fadedPieces,
            );
            await promise;

            expect(store.getState().removingPieces).toEqual(new Map());
        });

        it("should display initialSpawnPositions before showing newPieces", async () => {
            const movingPieceId = "moving";
            const spawnedPieceId = "spawn1";

            const initialSpawnPositions = BoardPieces.fromPieces(
                createFakePiece({ id: movingPieceId }),
                createFakePiece({ id: spawnedPieceId }),
            );

            const newPieces = BoardPieces.fromPieces(
                createFakePiece({ id: movingPieceId }),
                createFakePiece({ id: spawnedPieceId }),
            );

            const animation: AnimationStep = {
                newPieces,
                movedPieceIds: [movingPieceId, spawnedPieceId],
                initialSpawnPositions,
            };

            const promise = store.getState().playAnimation(animation);

            expect(store.getState().animatingPieces).toEqual(
                initialSpawnPositions,
            );

            await vi.runAllTimersAsync();
            await promise;

            expect(store.getState().animatingPieces).toBeNull();
        });

        it("should play audio for each animation step", async () => {
            const playAudioForAnimationStepMock = vi.fn();
            store.setState({
                playAudioForAnimationStep: playAudioForAnimationStepMock,
            });
            const piece = createFakePiece();
            const animationSteps: AnimationStep[] = [
                {
                    newPieces: BoardPieces.fromPieces(piece),
                    movedPieceIds: [piece.id],
                },
                {
                    newPieces: BoardPieces.fromPieces(piece),
                    movedPieceIds: [piece.id],
                    isCapture: true,
                },
            ];

            const promise = store.getState().playAnimation(animationSteps);

            expect(
                playAudioForAnimationStepMock,
            ).toHaveBeenCalledExactlyOnceWith(animationSteps[0]);

            vi.advanceTimersByTime(constants.PIECE_ANIMATION_LENGTH_MS);
            await flushMicrotasks();
            vi.advanceTimersByTime(constants.ANIMATION_STEP_DELAY_MS);
            await flushMicrotasks();
            expect(playAudioForAnimationStepMock).toHaveBeenCalledTimes(2);
            expect(playAudioForAnimationStepMock).toHaveBeenCalledWith(
                animationSteps[1],
            );

            await promise;
        });

        it("should set lastMove from moveBounds for each animation step", async () => {
            const moveBounds1: MoveBounds = {
                from: logicalPoint({ x: 0, y: 1 }),
                to: logicalPoint({ x: 0, y: 3 }),
            };
            const moveBounds3: MoveBounds = {
                from: logicalPoint({ x: 1, y: 1 }),
                to: logicalPoint({ x: 1, y: 2 }),
            };

            const piece = createFakePiece();
            const boardPieces = BoardPieces.fromPieces(piece);

            const animationSteps: AnimationStep[] = [
                {
                    newPieces: boardPieces,
                    movedPieceIds: [piece.id],
                    moveBounds: moveBounds1,
                },
                {
                    newPieces: boardPieces,
                    movedPieceIds: [piece.id],
                },
                {
                    newPieces: boardPieces,
                    movedPieceIds: [piece.id],
                    moveBounds: moveBounds3,
                },
            ];

            const promise = store.getState().playAnimation(animationSteps);

            expect(store.getState().lastMove).toEqual(moveBounds1);

            vi.advanceTimersByTime(constants.PIECE_ANIMATION_LENGTH_MS);
            await flushMicrotasks();
            vi.advanceTimersByTime(constants.ANIMATION_STEP_DELAY_MS);
            await flushMicrotasks();
            expect(store.getState().lastMove).toEqual(null);

            vi.advanceTimersByTime(constants.PIECE_ANIMATION_LENGTH_MS);
            await flushMicrotasks();
            vi.advanceTimersByTime(constants.ANIMATION_STEP_DELAY_MS);
            await flushMicrotasks();
            expect(store.getState().lastMove).toEqual(moveBounds3);

            await promise;

            expect(store.getState().lastMove).toEqual(moveBounds3);
        });

        it("should skip step delay if disableStepDelay is true", async () => {
            const piece1 = createFakePiece({ id: "1" });
            const piece2 = createFakePiece({ id: "2" });

            const step1: AnimationStep = {
                newPieces: BoardPieces.fromPieces(piece1),
                movedPieceIds: [piece1.id],
                disableStepDelay: true,
            };
            const step2: AnimationStep = {
                newPieces: BoardPieces.fromPieces(piece2),
                movedPieceIds: [piece2.id],
            };

            const promise = store.getState().playAnimation([step1, step2]);

            expect(store.getState().animatingPieces).toEqual(step1.newPieces);

            vi.advanceTimersByTime(constants.PIECE_ANIMATION_LENGTH_MS);
            await flushMicrotasks();

            expect(store.getState().animatingPieces).toEqual(step2.newPieces);

            await promise;
            expect(store.getState().animatingPieces).toBeNull();
        });

        it("should skip animation length delay if movedPieceIds is empty", async () => {
            const piece1 = createFakePiece({ id: "1" });
            const piece2 = createFakePiece({ id: "2" });

            const step1: AnimationStep = {
                newPieces: BoardPieces.fromPieces(piece1),
                movedPieceIds: [], // no pieces to animate
            };
            const step2: AnimationStep = {
                newPieces: BoardPieces.fromPieces(piece2),
                movedPieceIds: [piece2.id],
            };

            const promise = store.getState().playAnimation([step1, step2]);

            expect(store.getState().animatingPieces).toEqual(step1.newPieces);
            await flushMicrotasks();
            vi.advanceTimersByTime(constants.ANIMATION_STEP_DELAY_MS);
            await flushMicrotasks();

            expect(store.getState().animatingPieces).toEqual(step2.newPieces);

            vi.advanceTimersByTime(constants.PIECE_ANIMATION_LENGTH_MS);
            await flushMicrotasks();

            await promise;
            expect(store.getState().animatingPieces).toBeNull();
        });

        it("should play board effects", async () => {
            const piece1 = createFakePiece({ id: "1" });
            const piece2 = createFakePiece({ id: "2" });
            const boardEffect = createFakePawnThrowEffect();

            let resolveEffect!: () => void;
            const effectPromise = new Promise<void>((resolve) => {
                resolveEffect = resolve;
            });

            const addTransientBoardEffectMock = vi.fn(() => ({
                promise: effectPromise,
                id: "effect" as BoardEffectId,
            }));
            store.setState({
                addTransientBoardEffect: addTransientBoardEffectMock,
            });

            const step1: AnimationStep = {
                newPieces: BoardPieces.fromPieces(piece1),
                movedPieceIds: [piece1.id],
                boardEffect,
            };
            const step2: AnimationStep = {
                newPieces: BoardPieces.fromPieces(piece2),
                movedPieceIds: [piece2.id],
            };

            const animationPromise = store
                .getState()
                .playAnimation([step1, step2]);

            expect(store.getState().animatingPieces).toEqual(step1.newPieces);
            vi.advanceTimersByTime(constants.PIECE_ANIMATION_LENGTH_MS);
            await flushMicrotasks();
            vi.advanceTimersByTime(constants.ANIMATION_STEP_DELAY_MS);
            await flushMicrotasks();
            // it shouldn't continue until effect promise is resolved
            expect(store.getState().animatingPieces).toEqual(step1.newPieces);

            resolveEffect();
            await flushMicrotasks();
            vi.advanceTimersByTime(constants.PIECE_ANIMATION_LENGTH_MS);
            await flushMicrotasks();
            vi.advanceTimersByTime(constants.ANIMATION_STEP_DELAY_MS);
            await flushMicrotasks();

            expect(store.getState().animatingPieces).toEqual(step2.newPieces);

            await animationPromise;
            expect(store.getState().animatingPieces).toBeNull();
        });

        it("should resolve all transient effects at the start of", async () => {
            const piece = createFakePiece();

            const resolveAllTransientBoardEffectsMock = vi.fn();
            store.setState({
                resolveAllTransientBoardEffects:
                    resolveAllTransientBoardEffectsMock,
            });

            const animationStep: AnimationStep = {
                newPieces: BoardPieces.fromPieces(piece),
                movedPieceIds: [piece.id],
            };

            await store.getState().playAnimation(animationStep);

            expect(resolveAllTransientBoardEffectsMock).toHaveBeenCalledOnce();
        });
    });

    describe("animatePiece", () => {
        it("should animate a single piece and persist animatingPieces", async () => {
            const piece = createFakePiece();
            const newPosition = logicalPoint({ x: 2, y: 2 });
            const pieces = BoardPieces.fromPieces(piece);

            await store
                .getState()
                .animatePiece(piece.id, newPosition, new BoardPieces(pieces));

            pieces.addAt(piece, newPosition);
            const animatingMap = store.getState().animatingPieces;
            expect(animatingMap).toEqual(pieces);

            expect(store.getState().animatingPieceIds.size).toBe(0);
        });
    });

    describe("clearAnimation", () => {
        it("should clear animatingPieces", () => {
            const piece = createFakePiece();

            store.setState({
                animatingPieces: BoardPieces.fromPieces(piece),
            });

            store.getState().clearAnimation();

            expect(store.getState().animatingPieces).toBeNull();
        });
    });

    describe("resetLastMove", () => {
        it("should clear lastMove when called directly", () => {
            store.setState({
                lastMove: {
                    from: logicalPoint({ x: 0, y: 0 }),
                    to: logicalPoint({ x: 0, y: 1 }),
                },
            });

            store.getState().resetLastMove();

            expect(store.getState().lastMove).toBeNull();
        });
    });
});
