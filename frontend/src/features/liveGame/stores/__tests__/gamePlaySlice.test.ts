import { StoreApi } from "zustand";

import createLiveChessStore, {
    LiveChessStore,
    LiveChessStoreProps,
} from "../liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createFakeClock } from "@/lib/testUtils/fakers/clockFaker";
import { Clocks, GameColor } from "@/lib/apiClient";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";

describe("gamePlaySlice", () => {
    let store: StoreApi<LiveChessStore>;
    let initialProps: LiveChessStoreProps;

    beforeEach(() => {
        initialProps = createFakeLiveChessStoreProps();
        store = createLiveChessStore(initialProps);
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
                .receiveLiveMove(createFakeClock(), GameColor.WHITE);

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
