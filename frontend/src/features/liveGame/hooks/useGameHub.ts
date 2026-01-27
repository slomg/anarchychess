import { useMemo } from "react";

import {
    Clocks,
    DrawState,
    MoveSnapshot,
    GameResultData,
} from "@/lib/apiClient";

import useSignalREmitter from "@/features/signalR/hooks/useSignalREmitter";
import useSignalREvent from "@/features/signalR/hooks/useSignalREvent";
import { LogicalPoint } from "@/features/point/types";
import constants from "@/lib/constants";

export type GameClientEvents = {
    SyncRevisionAsync: [currentRevision: number];
    MoveMadeAsync: [
        move: MoveSnapshot,
        plyNumber: number,
        clocks: Clocks,
        didMoveEndGame: boolean,
    ];
    OpponentMoveMadeAsync: [
        move: MoveSnapshot,
        plyNumber: number,
        encodedLegalMoves: string,
        clocks: Clocks,
    ];
    ReceiveOvertimeAsync: [
        plyNumber: number,
        removedFrom: LogicalPoint,
        encodedLegalMoves: string,
    ];

    DrawStateChangeAsync: [drawState: DrawState];
    GameEndedAsync: [result: GameResultData, finalClocks: Clocks];

    ChatMessageAsync: [senderUsername: string, message: string];
    ChatMessageDeliveredAsync: [cooldownLeftMs: number];
    ChatConnectedAsync: [];

    RematchRequestedAsync: [];
    RematchCancelledAsync: [];
    RematchAccepted: [createdGameToken: string];
};

type GameHubEvents = {
    MovePieceAsync: [gameToken: string, key: string];

    EndGameAsync: [gameToken: string];
    RequestDrawAsync: [gameToken: string];
    DeclineDrawAsync: [gameToken: string];

    SendChatAsync: [gameToken: string, message: string];

    RequestRematchAsync: [gameToken: string];
    CancelRematchAsync: [gameToken: string];
};

export function useGameEvent<
    TEventName extends Extract<keyof GameClientEvents, string>,
>(
    gameToken: string,
    eventName: TEventName,
    onEvent?: (...args: GameClientEvents[TEventName]) => void,
) {
    const url = useMemo(() => {
        const u = new URL(constants.SIGNALR_PATHS.GAME);
        u.searchParams.append("gameToken", gameToken);
        return u.toString();
    }, [gameToken]);

    return useSignalREvent<GameClientEvents, TEventName>(
        url,
        eventName,
        onEvent,
    );
}

export function useGameEmitter(gameToken: string) {
    const url = useMemo(() => {
        const u = new URL(constants.SIGNALR_PATHS.GAME);
        u.searchParams.append("gameToken", gameToken);
        return u.toString();
    }, [gameToken]);

    return useSignalREmitter<GameHubEvents>(url);
}
