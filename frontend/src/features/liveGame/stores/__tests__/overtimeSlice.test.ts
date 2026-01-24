import { StoreApi } from "zustand";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createFakePlayerOvertime } from "@/lib/testUtils/fakers/playerOvertimeFaker";
import createLiveChessStore, { LiveChessStore } from "../liveChessStore";
import { GameColor } from "@/lib/apiClient";

describe("OvertimeSlice", () => {
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createLiveChessStore(createFakeLiveChessStoreProps());
    });

    describe("setOvertime", () => {
        it("should set the white overtime state", () => {
            const playerOvertime = createFakePlayerOvertime();
            const setOvertimeTurnStartedAt = 1234;

            store
                .getState()
                .setOvertime(
                    GameColor.WHITE,
                    playerOvertime,
                    setOvertimeTurnStartedAt,
                );

            const { whiteOvertime, blackOvertime, overtimeTurnStartedAt } =
                store.getState();
            expect(whiteOvertime).toEqual(playerOvertime);
            expect(blackOvertime).toBeNull();
            expect(overtimeTurnStartedAt).toBe(setOvertimeTurnStartedAt);
        });

        it("should set the black overtime state", () => {
            const playerOvertime = createFakePlayerOvertime();
            const setOvertimeTurnStartedAt = 1234;

            store
                .getState()
                .setOvertime(
                    GameColor.BLACK,
                    playerOvertime,
                    setOvertimeTurnStartedAt,
                );

            const { whiteOvertime, blackOvertime, overtimeTurnStartedAt } =
                store.getState();
            expect(blackOvertime).toEqual(playerOvertime);
            expect(whiteOvertime).toBeNull();
            expect(overtimeTurnStartedAt).toBe(setOvertimeTurnStartedAt);
        });
    });
});
