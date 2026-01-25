import { StoreApi } from "zustand";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import createLiveChessStore, { LiveChessStore } from "../liveChessStore";
import { GameColor } from "@/lib/apiClient";
import { createFakePendingOvertimeRemoval } from "@/lib/testUtils/fakers/pendingOvertimeRemovalFaker";

describe("OvertimeSlice", () => {
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createLiveChessStore(createFakeLiveChessStoreProps());
    });

    describe("setOvertime", () => {
        it("should set the white overtime state", () => {
            const playerOvertime = [
                createFakePendingOvertimeRemoval(),
                createFakePendingOvertimeRemoval(),
                createFakePendingOvertimeRemoval(),
            ];

            store.getState().setOvertime(GameColor.WHITE, playerOvertime);

            const { whiteOvertime, blackOvertime } = store.getState();
            expect(whiteOvertime).toEqual(playerOvertime);
            expect(blackOvertime).toBeNull();
        });

        it("should set the black overtime state", () => {
            const playerOvertime = [
                createFakePendingOvertimeRemoval(),
                createFakePendingOvertimeRemoval(),
                createFakePendingOvertimeRemoval(),
            ];

            store.getState().setOvertime(GameColor.BLACK, playerOvertime);

            const { whiteOvertime, blackOvertime } = store.getState();
            expect(blackOvertime).toEqual(playerOvertime);
            expect(whiteOvertime).toBeNull();
        });
    });
});
