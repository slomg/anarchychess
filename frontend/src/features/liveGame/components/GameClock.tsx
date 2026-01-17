import { useEffect, useEffectEvent, useRef, useState } from "react";
import clsx from "clsx";

import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import useLiveChessStore from "../hooks/useLiveChessStore";
import { GameColor } from "@/lib/apiClient";

const GameClock = ({ color }: { color: GameColor }) => {
    const clock = useLiveChessStore((x) =>
        color === GameColor.WHITE ? x.clocks.whiteClock : x.clocks.blackClock,
    );
    const viewer = useLiveChessStore((x) => x.viewer);
    const { sideToMove, serverClockAheadByMs, clockLastUpdated, isFrozen } =
        useLiveChessStore((x) => ({
            sideToMove: x.sideToMove,
            serverClockAheadByMs: x.serverClockAheadByMs,
            clockLastUpdated: x.clocks.lastUpdated,
            isFrozen: x.clocks.isFrozen,
        }));

    const [timeLeftMs, setTimeLeftMs] = useState<number>(clock.timeLeftMs);
    const [timeUntilAbandonedMs, setTimeUntilAbandonedMs] = useState<
        number | null
    >(clock.timeUntilAbandonMs ?? null);

    const playedWarningSoundRef = useRef<boolean>(false);

    const isInTimeTrouble = timeLeftMs < 20000;
    const isTicking = color === sideToMove && !isFrozen;

    function calculateTimePassed(
        lastUpdated: number,
        serverClockAheadByMs: number,
    ) {
        return new Date().valueOf() + serverClockAheadByMs - lastUpdated;
    }

    const updateTimeLeft = useEffectEvent(() => {
        if (!isTicking) {
            setTimeUntilAbandonedMs(null);
            setTimeLeftMs(clock.timeLeftMs);
            return;
        }

        const timePassed = calculateTimePassed(
            clockLastUpdated,
            serverClockAheadByMs,
        );

        if (!clock.isInGracePeriod) {
            setTimeLeftMs(clock.timeLeftMs - timePassed);
        }

        if (typeof clock.timeUntilAbandonMs === "number") {
            setTimeUntilAbandonedMs(clock.timeUntilAbandonMs - timePassed);
        } else {
            setTimeUntilAbandonedMs(null);
        }
    });

    useEffect(() => {
        updateTimeLeft();
    }, [clock.timeLeftMs, clock.timeUntilAbandonMs, clockLastUpdated]);

    useEffect(() => {
        if (!isTicking) return;

        const interval = setInterval(
            updateTimeLeft,
            isInTimeTrouble ? 100 : 1000,
        );
        return () => {
            clearInterval(interval);
        };
    }, [
        clock.timeLeftMs,
        clock.timeUntilAbandonMs,
        clockLastUpdated,
        isTicking,
        isInTimeTrouble,
        serverClockAheadByMs,
    ]);

    useEffect(() => {
        if (
            isFrozen ||
            !isInTimeTrouble ||
            playedWarningSoundRef.current ||
            viewer.playerColor !== color
        )
            return;

        AudioPlayer.playAudio(AudioType.LOW_TIME);
        playedWarningSoundRef.current = true;
    }, [timeLeftMs, color, viewer.playerColor, isFrozen, isInTimeTrouble]);

    const minutes = Math.max(0, Math.floor(timeLeftMs / 60000));
    const seconds = Math.max(0, (timeLeftMs % 60000) / 1000);

    const strMinutes = minutes.toString().padStart(2, "0");

    const strSeconds = isInTimeTrouble
        ? seconds.toFixed(2).padStart(5, "0") // xx.yy
        : Math.floor(seconds).toString().padStart(2, "0"); // xx

    return (
        <div
            className="flex h-full flex-col items-end justify-center gap-2
                font-mono"
        >
            <p
                className={clsx(
                    "text-2xl leading-4",
                    isInTimeTrouble && isTicking && "animate-freakout",
                    seconds <= 0 && minutes <= 0 && isFrozen && "text-red-600",
                )}
            >
                {strMinutes}:{strSeconds}
            </p>

            {timeUntilAbandonedMs && (
                <span className="leading-3 font-bold text-nowrap">
                    move in{" "}
                    {Math.max(0, Math.round(timeUntilAbandonedMs / 1000))}s
                </span>
            )}
        </div>
    );
};
export default GameClock;
