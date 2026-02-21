import useSignalREmitter from "@/features/signalR/hooks/useSignalREmitter";
import useSignalREvent from "@/features/signalR/hooks/useSignalREvent";
import { GameResultData, MoveSnapshot } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { useMemo } from "react";

export type BotClientEvents = {
    PlayerMadeMoveAsync: [
        move: MoveSnapshot,
        plyNumber: number,
        didMoveEndGame: boolean,
    ];
    BotMadeMoveAsync: [
        move: MoveSnapshot,
        plyNumber: number,
        compressedLegalMoves: string,
    ];
    GameEndedAsync: [result: GameResultData];
};

type BotHubEvents = {
    MakeMoveAsync: [gameToken: string, moveKey: string];
    ResignAsync: [gameToken: string];
};

export function useBotEvent<
    TEventName extends Extract<keyof BotClientEvents, string>,
>(
    gameToken: string,
    eventName: TEventName,
    onEvent?: (...args: BotClientEvents[TEventName]) => void,
) {
    const url = useMemo(() => {
        const u = new URL(constants.SIGNALR_PATHS.BOT);
        u.searchParams.append("gameToken", gameToken);
        return u.toString();
    }, [gameToken]);

    return useSignalREvent<BotClientEvents, TEventName>(
        url,
        eventName,
        onEvent,
    );
}

export function useBotEmitter(gameToken: string) {
    const url = useMemo(() => {
        const u = new URL(constants.SIGNALR_PATHS.BOT);
        u.searchParams.append("gameToken", gameToken);
        return u.toString();
    }, [gameToken]);

    return useSignalREmitter<BotHubEvents>(url);
}
