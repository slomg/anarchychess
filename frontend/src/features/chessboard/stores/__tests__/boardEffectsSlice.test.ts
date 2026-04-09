import { StoreApi } from "zustand";

import { createFakeThrowAimEffect } from "@/lib/testUtils/fakers/throwAimEffectFaker";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { logicalPoint } from "@/features/point/pointUtils";
import { BoardEffectId } from "../boardEffectsSlice";
import { createFakePawnThrowEffect } from "@/lib/testUtils/fakers/pawnThrowEffectFaker";

describe("BoardEffectsSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    describe("addPersistentBoardEffect", () => {
        it("should add an effect and return a valid id", () => {
            const effect = createFakeThrowAimEffect();
            const id = store.getState().addPersistentBoardEffect(effect);

            const activeEffects = store.getState().activePersistentBoardEffects;

            expect(id).toBeDefined();
            expect(activeEffects.size).toBe(1);
            expect(activeEffects.get(id)).toEqual(effect);
        });
    });

    describe("updatePersistentBoardEffect", () => {
        it("should update an existing effect", () => {
            const initial = createFakeThrowAimEffect({
                to: logicalPoint({ x: 1, y: 1 }),
            });
            const updated = {
                ...initial,
                to: logicalPoint({ x: 2, y: 2 }),
            };
            const id = store.getState().addPersistentBoardEffect(initial);

            store.getState().updatePersistentBoardEffect(id, updated);

            const activeEffects = store.getState().activePersistentBoardEffects;
            expect(activeEffects.size).toBe(1);
            expect(activeEffects.get(id)).toEqual(updated);
        });

        it("should do nothing if id does not exist", () => {
            const effect = createFakeThrowAimEffect();

            store
                .getState()
                .updatePersistentBoardEffect(
                    "wrong id" as BoardEffectId,
                    effect,
                );

            const activeEffects = store.getState().activePersistentBoardEffects;
            expect(activeEffects.size).toBe(0);
        });
    });

    describe("removePersistentBoardEffect", () => {
        it("should remove an existing effect", () => {
            const effect = createFakeThrowAimEffect();
            const id = store.getState().addPersistentBoardEffect(effect);

            store.getState().removePersistentBoardEffect(id);

            const activeEffects = store.getState().activePersistentBoardEffects;
            expect(activeEffects.size).toBe(0);
            expect(activeEffects.has(id)).toBe(false);
        });

        it("should do nothing if id does not exist", () => {
            const effect = createFakeThrowAimEffect();
            store.getState().addPersistentBoardEffect(effect);

            store
                .getState()
                .removePersistentBoardEffect("wrong id" as BoardEffectId);

            const activeEffects = store.getState().activePersistentBoardEffects;
            expect(activeEffects.size).toBe(1);
        });
    });

    describe("addTransientBoardEffect", () => {
        it("should add an effect and return id and promise", async () => {
            const effect = createFakePawnThrowEffect();
            const { id, promise } = store
                .getState()
                .addTransientBoardEffect(effect);

            const activeEffects = store.getState().activeTransientBoardEffects;
            expect(id).toBeDefined();
            expect(activeEffects.size).toBe(1);
            expect(activeEffects.get(id)?.value).toEqual(effect);

            store.getState().resolveTransientBoardEffect(id);
            await expect(promise).resolves.toBeUndefined();
            expect(store.getState().activeTransientBoardEffects.has(id)).toBe(
                false,
            );
        });

        it("should allow settling the effect without removing it", async () => {
            const effect = createFakePawnThrowEffect();
            const { id, promise } = store
                .getState()
                .addTransientBoardEffect(effect);

            store.getState().activeTransientBoardEffects.get(id)!.settle();

            await expect(promise).resolves.toBeUndefined();

            const activeEffects = store.getState().activeTransientBoardEffects;
            expect(activeEffects.has(id)).toBe(true);
            expect(activeEffects.get(id)?.value).toEqual(effect);
        });
    });

    describe("resolveTransientBoardEffect", () => {
        it("should complete and remove the effect", async () => {
            const effect = createFakePawnThrowEffect();
            const { id, promise } = store
                .getState()
                .addTransientBoardEffect(effect);

            store.getState().resolveTransientBoardEffect(id);

            await expect(promise).resolves.toBeUndefined();
            const activeEffects = store.getState().activeTransientBoardEffects;
            expect(activeEffects.has(id)).toBe(false);
        });

        it("should do nothing if id does not exist", () => {
            store
                .getState()
                .resolveTransientBoardEffect("wrong id" as BoardEffectId);
            const activeEffects = store.getState().activeTransientBoardEffects;
            expect(activeEffects.size).toBe(0);
        });
    });

    describe("resolveAllTransientBoardEffects", () => {
        it("should complete and remove all active transient effects", async () => {
            const effect1 = createFakePawnThrowEffect();
            const effect2 = createFakePawnThrowEffect();

            const { id: id1, promise: promise1 } = store
                .getState()
                .addTransientBoardEffect(effect1);
            const { id: id2, promise: promise2 } = store
                .getState()
                .addTransientBoardEffect(effect2);

            store.getState().resolveAllTransientBoardEffects();

            await expect(promise1).resolves.toBeUndefined();
            await expect(promise2).resolves.toBeUndefined();

            const activeEffects = store.getState().activeTransientBoardEffects;
            expect(activeEffects.size).toBe(0);
            expect(activeEffects.has(id1)).toBe(false);
            expect(activeEffects.has(id2)).toBe(false);
        });

        it("should do nothing if there are no active transient effects", () => {
            store.getState().resolveAllTransientBoardEffects();

            const activeEffects = store.getState().activeTransientBoardEffects;
            expect(activeEffects.size).toBe(0);
        });
    });
});
