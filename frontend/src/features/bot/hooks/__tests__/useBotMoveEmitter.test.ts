import { renderHook } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import {
    createFakeBoardPieces,
    createFakeMove,
} from "@/lib/testUtils/fakers/chessboardFakers";

import useMoveEmitterForLiveGames from "@/features/liveGame/hooks/useMoveEmitterForLiveGames";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import createDeferred from "@/lib/testUtils/createDeferred";
import useBotMoveEmitter from "../useBotMoveEmitter";
import { useBotEmitter } from "../useBotHub";

vi.mock("@/features/liveGame/hooks/useMoveEmitterForLiveGames");
vi.mock("../useBotHub");

describe("useBotMoveEmitter", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    const sendBotEventMock = vi.fn();
    const useBotEmitterMock = vi.mocked(useBotEmitter);
    const useMoveEmitterForLiveGamesMock = vi.mocked(
        useMoveEmitterForLiveGames,
    );

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        useBotEmitterMock.mockReturnValue(sendBotEventMock);
    });

    it("should call useMoveEmitterForLiveGames with a wrapped sendGameEvent", async () => {
        renderHook(() => useBotMoveEmitter(liveChessStore, chessboardStore));

        expect(useMoveEmitterForLiveGames).toHaveBeenCalledOnce();
        const [, , sendMoveCallback] =
            useMoveEmitterForLiveGamesMock.mock.calls[0];

        const { promise: animationPromise, resolve: resolveAnimation } =
            createDeferred();
        const move = createFakeMove();
        sendMoveCallback({
            move,
            prevPieces: createFakeBoardPieces(),
            animationPromise,
        });
        await flushMicrotasks();

        expect(sendBotEventMock).not.toHaveBeenCalled();

        resolveAnimation();
        await flushMicrotasks();

        expect(sendBotEventMock).toHaveBeenCalledWith(
            "MakeMoveAsync",
            liveChessStore.getState().gameToken,
            move.moveKey,
        );
    });
});
