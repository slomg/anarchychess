import { StoreApi } from "zustand";

import createLiveChessStore, { LiveChessStore } from "../liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createFakeClocks } from "@/lib/testUtils/fakers/clocksFaker";
import { Clocks, GameColor } from "@/lib/apiClient";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";

describe("gamePlaySlice", () => {
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createLiveChessStore(createFakeLiveChessStoreProps());
    });

    describe("initState", () => {
        it("should set serverClockAheadByMs based on server time - current time", () => {
            const fakeServerTime = 100;
            const fakeNow = 95;
            vi.setSystemTime(fakeNow);

            const store = createLiveChessStore(
                createFakeLiveChessStoreProps({
                    clocks: createFakeClocks({ serverTime: fakeServerTime }),
                }),
            );

            const expectedDrift = fakeServerTime - fakeNow;
            expect(store.getState().serverClockAheadByMs).toBe(expectedDrift);
        });
    });

    describe("isInteractionAllowed", () => {
        it("should allow interaction when the game is over", () => {
            store.setState({
                resultData: createFakeGameResultData(),
                viewer: {
                    userId: "user id",
                    playerColor: null,
                },
                sideToMove: GameColor.WHITE,
            });

            const result = store.getState().isInteractionAllowed();
            expect(result).toBe(true);
        });

        it("should allow interaction when it is the viewer's turn", () => {
            store.setState({
                resultData: null,
                viewer: {
                    userId: "user id",
                    playerColor: GameColor.BLACK,
                },
                sideToMove: GameColor.BLACK,
            });

            const result = store.getState().isInteractionAllowed();
            expect(result).toBe(true);
        });

        it("should not allow interaction when it is not the viewer's turn", () => {
            store.setState({
                resultData: null,
                viewer: {
                    userId: "user id",
                    playerColor: GameColor.WHITE,
                },
                sideToMove: GameColor.BLACK,
            });

            const result = store.getState().isInteractionAllowed();
            expect(result).toBe(false);
        });

        it("should not allow interaction for spectators when the game is not over", () => {
            store.setState({
                resultData: null,
                viewer: {
                    userId: "user id",
                    playerColor: null,
                },
                sideToMove: GameColor.WHITE,
            });

            const result = store.getState().isInteractionAllowed();
            expect(result).toBe(false);
        });

        it("should allow interaction for spectators when the game is over", () => {
            store.setState({
                resultData: createFakeGameResultData(),
                viewer: {
                    userId: "user-1",
                    playerColor: null,
                },
                sideToMove: GameColor.BLACK,
            });

            const result = store.getState().isInteractionAllowed();
            expect(result).toBe(true);
        });
    });

    describe("receiveLiveMove", () => {
        it("should update clocks, sideToMove, and clear isPendingMoveAck", () => {
            const newClocks: Clocks = createFakeClocks({ serverTime: 1 });
            const newSideToMove = GameColor.BLACK;

            store.setState({
                isPendingMoveAck: true,
                clocks: createFakeClocks({ serverTime: 2 }),
                sideToMove: GameColor.WHITE,
            });

            store.getState().receiveLiveMove(newClocks, newSideToMove);
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

            store
                .getState()
                .receiveLiveMove(createFakeClocks(), GameColor.WHITE);

            expect(decrementDrawCooldownMock).toHaveBeenCalledOnce();
        });

        it("should update serverClockAheadByMs based on the new server time", () => {
            const fakeNow = 1000;
            vi.setSystemTime(fakeNow);

            store.setState({ serverClockAheadByMs: 0 });

            const newServerTime = fakeNow + 2000;
            const newClocks = createFakeClocks({
                serverTime: newServerTime,
            });

            store.getState().receiveLiveMove(newClocks, GameColor.BLACK);

            const state = store.getState();
            expect(state.serverClockAheadByMs).toBe(newServerTime - fakeNow);
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
            const oldClocks = createFakeClocks({ serverTime: 1 });
            store.setState({ clocks: oldClocks });

            const newClocks = createFakeClocks({ serverTime: 2 });
            store.getState().setClocks(newClocks);

            expect(store.getState().clocks).toEqual(newClocks);
        });
    });
});
