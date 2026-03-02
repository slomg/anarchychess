import { renderHook } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import useMoveEmitterForLiveGames from "../useMoveEmitterForLiveGames";

import useLiveMoveEmitter from "../useLiveMoveEmitter";
import { useGameEmitter } from "../useGameHub";
import { MoveKey } from "@/features/chessboard/lib/types";

vi.mock("@/features/liveGame/hooks/useGameHub");
vi.mock("../useMoveEmitterForLiveGames");

describe("useLiveMoveEmitter", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    const sendGameEventMock = vi.fn();

    const useGameEmitterMock = vi.mocked(useGameEmitter);
    const useMoveEmitterForLiveGamesMock = vi.mocked(
        useMoveEmitterForLiveGames,
    );

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        useGameEmitterMock.mockReturnValue(sendGameEventMock);
    });

    it("should call useMoveEmitterForLiveGames with a wrapped sendGameEvent", () => {
        renderHook(() => useLiveMoveEmitter(liveChessStore, chessboardStore));

        expect(useMoveEmitterForLiveGames).toHaveBeenCalledOnce();
        const [, , sendMoveCallback] =
            useMoveEmitterForLiveGamesMock.mock.calls[0];

        sendMoveCallback("move123" as MoveKey);
        expect(sendGameEventMock).toHaveBeenCalledWith(
            "MovePieceAsync",
            liveChessStore.getState().gameToken,
            "move123",
        );
    });

    it("should use the gameToken from the store", () => {
        liveChessStore.setState({ gameToken: "newToken" });

        renderHook(() => useLiveMoveEmitter(liveChessStore, chessboardStore));

        const [, , sendMoveCallback] =
            useMoveEmitterForLiveGamesMock.mock.calls[0];
        sendMoveCallback("move456" as MoveKey);

        expect(sendGameEventMock).toHaveBeenCalledWith(
            "MovePieceAsync",
            "newToken",
            "move456",
        );
    });
});
