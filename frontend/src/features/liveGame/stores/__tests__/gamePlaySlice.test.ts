import { StoreApi } from "zustand";

import createLiveChessStore, {
    LiveChessStore,
    LiveChessStoreProps,
} from "../liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createFakeClock } from "@/lib/testUtils/fakers/clockFaker";
import { Clocks, GameColor } from "@/lib/apiClient";

describe("gamePlaySlice", () => {
    let store: StoreApi<LiveChessStore>;
    let initialProps: LiveChessStoreProps;

    beforeEach(() => {
        initialProps = createFakeLiveChessStoreProps();
        store = createLiveChessStore(initialProps);
    });

    describe("receiveMove", () => {
        it("should update clocks, sideToMove, and clear isPendingMoveAck", () => {
            const newClocks: Clocks = {
                whiteClock: 500,
                blackClock: 600,
                lastUpdated: Date.now().valueOf(),
                isFrozen: true,
            };
            const newSideToMove = GameColor.BLACK;

            store.setState({
                isPendingMoveAck: true,
                clocks: {
                    whiteClock: 100,
                    blackClock: 200,
                    lastUpdated: Date.now().valueOf(),
                    isFrozen: false,
                },
                sideToMove: GameColor.WHITE,
            });

            store.getState().receiveMove(newClocks, newSideToMove);
            const state = store.getState();

            expect(state.clocks).toBe(newClocks);
            expect(state.sideToMove).toBe(newSideToMove);
            expect(state.isPendingMoveAck).toBe(false);
        });

        it("should call decrementDrawCooldown", () => {
            const decrementDrawCooldownMock = vi.fn();
            store.setState({
                decrementDrawCooldown: decrementDrawCooldownMock,
            });

            store.getState().receiveMove(createFakeClock(), GameColor.WHITE);

            expect(decrementDrawCooldownMock).toHaveBeenCalledOnce();
        });
    });

    describe("markPendingMoveAck", () => {
        it.each([true, false])(
            "should set isPendingMoveAck to true",
            (initial) => {
                store.setState({ isPendingMoveAck: initial });
                store.getState().markPendingMoveAck();
                expect(store.getState().isPendingMoveAck).toBe(true);
            },
        );
    });

    describe("setClocks", () => {
        it("should update clocks", () => {
            const oldClocks: Clocks = {
                whiteClock: 10,
                blackClock: 20,
                lastUpdated: 1000,
                isFrozen: false,
            };
            store.setState({ clocks: oldClocks });

            const newClocks: Clocks = {
                whiteClock: 1,
                blackClock: 2,
                lastUpdated: 500,
                isFrozen: true,
            };
            store.getState().setClocks(newClocks);

            expect(store.getState().clocks).toEqual(newClocks);
        });
    });
});
