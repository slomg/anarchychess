import ReactThreeTestRenderer from "@react-three/test-renderer";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createFakePawnThrowEffect } from "@/lib/testUtils/fakers/pawnThrowEffectFaker";
import { createFakeThrowAimEffect } from "@/lib/testUtils/fakers/throwAimEffectFaker";
import BoardEffects from "../BoardEffects";
import ThrowAimLine from "../ThrowAimLine";
import PawnThrow from "../PawnThrow";

vi.mock("../PawnThrow");
vi.mock("../ThrowAimLine");

describe("BoardEffects", () => {
    let store: StoreApi<ChessboardStore>;

    const throwAimLineMock = vi.mocked(ThrowAimLine);
    const pawnThrowMock = vi.mocked(PawnThrow);

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should render a persistent effect when present", async () => {
        const effect = createFakeThrowAimEffect();
        store.getState().addPersistentBoardEffect(effect);

        await ReactThreeTestRenderer.create(
            <ChessboardStoreContext.Provider value={store}>
                <BoardEffects />,
            </ChessboardStoreContext.Provider>,
        );

        expect(throwAimLineMock).toHaveBeenCalledWith({ effect }, undefined);
        expect(pawnThrowMock).not.toHaveBeenCalled();
    });

    it("should render a transient effect when present", async () => {
        const effect = createFakePawnThrowEffect();
        const { id } = store.getState().addTransientBoardEffect(effect);
        const finish = store
            .getState()
            .activeTransientBoardEffects.get(id)?.finish;

        await ReactThreeTestRenderer.create(
            <ChessboardStoreContext.Provider value={store}>
                <BoardEffects />
            </ChessboardStoreContext.Provider>,
        );

        expect(pawnThrowMock).toHaveBeenCalledWith(
            { effect, onFinish: finish },
            undefined,
        );
        expect(throwAimLineMock).not.toHaveBeenCalled();
    });

    it("should render both persistent and transient effects together", async () => {
        const persistent = createFakeThrowAimEffect();
        const transient = createFakePawnThrowEffect();

        const { addPersistentBoardEffect, addTransientBoardEffect } =
            store.getState();
        addPersistentBoardEffect(persistent);
        addTransientBoardEffect(transient);

        await ReactThreeTestRenderer.create(
            <ChessboardStoreContext.Provider value={store}>
                <BoardEffects />
            </ChessboardStoreContext.Provider>,
        );

        expect(throwAimLineMock).toHaveBeenCalled();
        expect(pawnThrowMock).toHaveBeenCalled();
    });
});
