import { StoreApi } from "zustand";

import { ChessboardStore, createChessboardStore } from "../chessboardStore";

describe("PiecesSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    describe("setSetupMode", () => {
        it("should set isSetupMode", () => {
            store.setState({ isSetupMode: false });
            const setSetupMode = store.getState().setSetupMode;

            setSetupMode(true);
            expect(store.getState().isSetupMode).toBe(true);

            setSetupMode(false);
            expect(store.getState().isSetupMode).toBe(false);
        });
    });
});
