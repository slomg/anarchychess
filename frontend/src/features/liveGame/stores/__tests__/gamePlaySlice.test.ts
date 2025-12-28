import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
    LiveChessStoreProps,
} from "../liveChessStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { Clocks, GameColor } from "@/lib/apiClient";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";
import { createFakeLegalMoves } from "@/lib/testUtils/fakers/chessboardFakers";
import { createFakeClock } from "@/lib/testUtils/fakers/clockFaker";
import LegalMoves from "@/features/chessboard/lib/legalMoves";

describe("gamePlaySlice", () => {
    let store: StoreApi<LiveChessStore>;
    let initialProps: LiveChessStoreProps;

    beforeEach(() => {
        initialProps = createFakeLiveChessStoreProps();
        store = createLiveChessStore(initialProps);
    });

    describe("receiveMove", () => {
        it("should update clocks, sideToMove, and clear isPendingMoveAck", () => {
            const newPosition = createFakePosition();
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

            store.getState().receiveMove(newPosition, newClocks, newSideToMove);
            const state = store.getState();

            expect(state.clocks).toBe(newClocks);
            expect(state.sideToMove).toBe(newSideToMove);
            expect(state.isPendingMoveAck).toBe(false);
        });

        it("should call receivePosition and decrementDrawCooldown", () => {
            const decrementMock = vi.fn();
            const receivePosSpy = vi.fn();

            store.setState({
                decrementDrawCooldown: decrementMock,
                receivePosition: receivePosSpy,
            });

            const newPosition = createFakePosition();
            store
                .getState()
                .receiveMove(newPosition, createFakeClock(), GameColor.WHITE);

            expect(decrementMock).toHaveBeenCalledOnce();
            expect(receivePosSpy).toHaveBeenCalledExactlyOnceWith(newPosition);
        });
    });

    describe("resetLegalMovesForOpponentTurn", () => {
        it("should reset latestMoveOptions", () => {
            const testLegalMoves = createFakeLegalMoves();
            store.setState({ latestLegalMoves: testLegalMoves });

            store.getState().resetLegalMovesForOpponentTurn();

            expect(store.getState().latestLegalMoves).toEqual(new LegalMoves());
        });
    });

    describe("receiveLegalMoves", () => {
        it("should update latestMoveOptions", () => {
            const newMoves = createFakeLegalMoves({ hasForcedMoves: true });

            store.getState().receiveLegalMoves(newMoves);
            expect(store.getState().latestLegalMoves).toBe(newMoves);
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
