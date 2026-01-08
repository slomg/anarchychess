import { useCallback, useEffect } from "react";

import { poolKeyToStr } from "../lib/matchmakingKeys";
import { PoolKey, PoolType } from "@/lib/apiClient";
import useLobbyStore from "../stores/lobbyStore";
import constants from "@/lib/constants";
import { useLobbyEmitter, useLobbyEvent } from "./useLobbyHub";

export default function useMatchmaking(pool: PoolKey): {
    createSeek: () => Promise<void>;
    cancelSeek: () => Promise<void>;
    toggleSeek: () => Promise<void>;
    isSeeking: boolean;
} {
    const sendLobbyEvent = useLobbyEmitter();
    const poolKeyStr = poolKeyToStr(pool);

    const { isSeeking, addSeek, removeSeek } = useLobbyStore((x) => ({
        isSeeking: x.seeks.has(poolKeyStr),
        addSeek: x.addSeek,
        removeSeek: x.removeSeek,
    }));

    useLobbyEvent("SeekFailedAsync", (pool) => {
        if (poolKeyToStr(pool) === poolKeyStr) removeSeek(poolKeyStr);
    });

    async function createSeek(): Promise<void> {
        addSeek(poolKeyStr);
        await sendSeekRequest();
    }

    async function cancelSeek(): Promise<void> {
        await sendLobbyEvent("CancelSeekAsync", pool);
        removeSeek(poolKeyStr);
    }

    const sendSeekRequest = useCallback(async () => {
        switch (pool.poolType) {
            case PoolType.RATED:
                await sendLobbyEvent("SeekRatedAsync", pool.timeControl);
                break;
            case PoolType.CASUAL:
                await sendLobbyEvent("SeekCasualAsync", pool.timeControl);
                break;
            default:
                throw new Error(`Unknown pool type ${pool}`);
        }
    }, [pool, sendLobbyEvent]);

    useEffect(() => {
        if (!isSeeking) return;

        const interval = setInterval(
            sendSeekRequest,
            constants.SEEK_RESUBSCRIBE_INTERAVAL_MS,
        );

        return () => clearInterval(interval);
    }, [isSeeking, sendSeekRequest]);

    function toggleSeek(): Promise<void> {
        if (isSeeking) return cancelSeek();
        else return createSeek();
    }

    return { createSeek, cancelSeek, toggleSeek, isSeeking };
}
