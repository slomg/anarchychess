import { useGameEmitter, useGameEvent } from "./useGameHub";
import useLiveChessStore from "./useLiveChessStore";

export default function useRematch() {
    const {
        gameToken,
        setRematchRequested,
        setRequestingRematch,
        isRequestingRematch,
        isRematchRequested,
    } = useLiveChessStore((x) => ({
        gameToken: x.gameToken,
        isRequestingRematch: x.isRequestingRematch,
        isRematchRequested: x.isRematchRequested,
        setRematchRequested: x.setRematchRequested,
        setRequestingRematch: x.setRequestingRematch,
    }));
    const sendGameEvent = useGameEmitter(gameToken);

    useGameEvent(gameToken, "RematchRequestedAsync", () =>
        setRematchRequested(true),
    );
    useGameEvent(gameToken, "RematchCancelledAsync", () =>
        setRematchRequested(false),
    );

    async function requestRematch() {
        await sendGameEvent("RequestRematchAsync", gameToken);
        setRequestingRematch(true);
    }

    async function cancelRematch() {
        await sendGameEvent("CancelRematchAsync", gameToken);
        setRequestingRematch(false);
    }

    async function toggleRematch() {
        if (isRequestingRematch) await cancelRematch();
        else await requestRematch();
    }

    return {
        requestRematch,
        cancelRematch,
        toggleRematch,
        isRequestingRematch,
        isRematchRequested,
    };
}
