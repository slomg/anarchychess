import { renderHook } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";

import useMoveEmitterForLiveGames from "@/features/liveGame/hooks/useMoveEmitterForLiveGames";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { MoveKey } from "@/features/chessboard/lib/types";
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

    it("should call useMoveEmitterForLiveGames with a wrapped sendGameEvent", () => {
        renderHook(() => useBotMoveEmitter(liveChessStore, chessboardStore));

        expect(useMoveEmitterForLiveGames).toHaveBeenCalledOnce();
        const [, , sendMoveCallback] =
            useMoveEmitterForLiveGamesMock.mock.calls[0];

        sendMoveCallback("move123" as MoveKey);
        expect(sendBotEventMock).toHaveBeenCalledWith(
            "MakeMoveAsync",
            liveChessStore.getState().gameToken,
            "move123",
        );
    });

    it("should use the gameToken from the store", () => {
        liveChessStore.setState({ gameToken: "newToken" });

        renderHook(() => useBotMoveEmitter(liveChessStore, chessboardStore));

        const [, , sendMoveCallback] =
            useMoveEmitterForLiveGamesMock.mock.calls[0];
        sendMoveCallback("move456" as MoveKey);

        expect(sendBotEventMock).toHaveBeenCalledWith(
            "MakeMoveAsync",
            "newToken",
            "move456",
        );
    });
});
