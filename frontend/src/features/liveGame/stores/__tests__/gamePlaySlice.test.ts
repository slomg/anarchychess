import { StoreApi } from "zustand";

import createLiveChessStore, { LiveChessStore } from "../liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";
import { createFakeClocks } from "@/lib/testUtils/fakers/clocksFaker";
import { ClockSnapshot } from "../../lib/types";
import { GameColor } from "@/lib/apiClient";

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
                    liveClocks: createFakeClocks({
                        serverTime: fakeServerTime,
                    }),
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
        it("should update sideToMove and clear isPendingMoveAck", () => {
            const newSideToMove = GameColor.BLACK;
            const plyNumber = 2;

            store.setState({
                isPendingMoveAck: true,
                sideToMove: GameColor.WHITE,
            });

            store
                .getState()
                .receiveLiveMove(plyNumber, createFakeClocks(), newSideToMove);
            const state = store.getState();

            expect(state.sideToMove).toBe(newSideToMove);
            expect(state.isPendingMoveAck).toBe(false);
        });

        it("should decrement cooldown", () => {
            const decrementDrawCooldownMock = vi.fn();
            store.setState({
                decrementDrawCooldown: decrementDrawCooldownMock,
            });
            const plyNumber = 3;

            store
                .getState()
                .receiveLiveMove(
                    plyNumber,
                    createFakeClocks(),
                    GameColor.WHITE,
                );

            expect(decrementDrawCooldownMock).toHaveBeenCalledOnce();
        });

        it("should set clocks", () => {
            const setClocksMock = vi.fn();
            store.setState({
                setClocks: setClocksMock,
            });
            const newClocks = createFakeClocks();
            const plyNumber = 3;

            store
                .getState()
                .receiveLiveMove(plyNumber, newClocks, GameColor.WHITE);

            expect(setClocksMock).toHaveBeenCalledExactlyOnceWith(
                plyNumber,
                newClocks,
            );
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
        it("should update live clocks", () => {
            const oldClocks = createFakeClocks({ serverTime: 1 });
            store.setState({ liveClocks: oldClocks });

            const newClocks = createFakeClocks({ serverTime: 2 });
            store.getState().setClocks(1, newClocks);

            expect(store.getState().liveClocks).toEqual(newClocks);
        });

        it("should update serverClockAheadByMs based on the new server time", () => {
            const fakeNow = 1000;
            vi.setSystemTime(fakeNow);

            store.setState({ serverClockAheadByMs: 0 });

            const newServerTime = fakeNow + 2000;
            const newClocks = createFakeClocks({
                serverTime: newServerTime,
            });

            store.getState().setClocks(1, newClocks);

            const state = store.getState();
            expect(state.serverClockAheadByMs).toBe(newServerTime - fakeNow);
        });

        it("should add the snapshot", () => {
            const newClocks = createFakeClocks();
            const plyNumber = 3;

            store.getState().setClocks(plyNumber, newClocks);

            expect(
                store.getState().clockSnapshotByPly.get(plyNumber),
            ).toEqual<ClockSnapshot>({
                whiteClock: newClocks.whiteClock.timeLeftMs,
                blackClock: newClocks.blackClock.timeLeftMs,
            });
        });
    });

    describe("getClockSnapshot", () => {
        it("should return null if the game is ongoing ", () => {
            store.setState({
                resultData: null,
                clockSnapshotByPly: new Map([
                    [1, { whiteClock: 100, blackClock: 200 }],
                ]),
            });

            expect(store.getState().getClockSnapshot(1)).toBeNull();
            expect(store.getState().getClockSnapshot(0)).toBeNull();
        });

        it("should return the correct snapshot if game is over", () => {
            const ply0: ClockSnapshot = {
                whiteClock: 300_000,
                blackClock: 300_000,
            };
            const ply1: ClockSnapshot = {
                whiteClock: 295_000,
                blackClock: 300_000,
            };
            store.setState({
                resultData: createFakeGameResultData(),
                clockSnapshotByPly: new Map([
                    [0, ply0],
                    [1, ply1],
                ]),
            });

            expect(store.getState().getClockSnapshot(0)).toEqual(ply0);
            expect(store.getState().getClockSnapshot(1)).toEqual(ply1);
        });

        it("should return null if game is over but snapshot for ply does not exist", () => {
            store.setState({
                resultData: createFakeGameResultData(),
                clockSnapshotByPly: new Map([
                    [0, { whiteClock: 300_000, blackClock: 300_000 }],
                ]),
            });

            expect(store.getState().getClockSnapshot(1)).toBeNull();
        });
    });
});
