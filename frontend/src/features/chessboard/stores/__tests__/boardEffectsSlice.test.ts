import { StoreApi } from "zustand";

import { createFakeThrowAimEffect } from "@/lib/testUtils/fakers/throwAimEffectFaker";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { logicalPoint } from "@/features/point/pointUtils";
import { BoardEffectId } from "../boardEffectsSlice";

describe("BoardEffectsSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    describe("addBoardEffect", () => {
        it("should add an effect and return a valid id", () => {
            const effect = createFakeThrowAimEffect();
            const id = store.getState().addBoardEffect(effect);

            const activeEffects = store.getState().activeBoardEffects;

            expect(id).toBeDefined();
            expect(activeEffects.size).toBe(1);
            expect(activeEffects.get(id)).toEqual(effect);
        });
    });

    describe("updateBoardEffect", () => {
        it("should update an existing effect", () => {
            const initial = createFakeThrowAimEffect({
                to: logicalPoint({ x: 1, y: 1 }),
            });
            const updated = {
                ...initial,
                to: logicalPoint({ x: 2, y: 2 }),
            };
            const id = store.getState().addBoardEffect(initial);

            store.getState().updateBoardEffect(id, updated);

            const activeEffects = store.getState().activeBoardEffects;
            expect(activeEffects.size).toBe(1);
            expect(activeEffects.get(id)).toEqual(updated);
        });

        it("should do nothing if id does not exist", () => {
            const effect = createFakeThrowAimEffect();

            store
                .getState()
                .updateBoardEffect("wrong id" as BoardEffectId, effect);

            const activeEffects = store.getState().activeBoardEffects;
            expect(activeEffects.size).toBe(0);
        });
    });

    describe("removeBoardEffect", () => {
        it("should remove an existing effect", () => {
            const effect = createFakeThrowAimEffect();
            const id = store.getState().addBoardEffect(effect);

            store.getState().removeBoardEffect(id);

            const activeEffects = store.getState().activeBoardEffects;
            expect(activeEffects.size).toBe(0);
            expect(activeEffects.has(id)).toBe(false);
        });

        it("should do nothing if id is null or undefined", () => {
            const effect = createFakeThrowAimEffect();
            store.getState().addBoardEffect(effect);

            store.getState().removeBoardEffect(null);
            store.getState().removeBoardEffect(undefined);

            const activeEffects = store.getState().activeBoardEffects;
            expect(activeEffects.size).toBe(1);
        });

        it("should do nothing if id does not exist", () => {
            const effect = createFakeThrowAimEffect();
            store.getState().addBoardEffect(effect);

            store.getState().removeBoardEffect("wrong id" as BoardEffectId);

            const activeEffects = store.getState().activeBoardEffects;
            expect(activeEffects.size).toBe(1);
        });
    });
});
