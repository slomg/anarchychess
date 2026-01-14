import { useEffect, useEffectEvent, useRef, useState } from "react";
import clsx from "clsx";

import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import useLiveChessStore from "../hooks/useLiveChessStore";
import { GameColor } from "@/lib/apiClient";

const GameClock = ({ color }: { color: GameColor }) => {
    const clocks = useLiveChessStore((x) => x.clocks);
    const viewer = useLiveChessStore((x) => x.viewer);
    const sideToMove = useLiveChessStore((x) => x.sideToMove);

    const playedWarningSoundRef = useRef<boolean>(false);

    const baseTimeLeft =
        color === GameColor.WHITE ? clocks.whiteClock : clocks.blackClock;
    const isTicking = sideToMove === color && !clocks.isFrozen;

    const [timeLeft, setTimeLeft] = useState<number>(baseTimeLeft);
    const isInTimeTrouble = timeLeft < 20000;

    const initializeNewTimeLeft = useEffectEvent(
        (baseTimeLeft: number, lastUpdated: number, isTicking: boolean) => {
            if (isTicking) {
                const timePassed = new Date().valueOf() - lastUpdated;
                setTimeLeft(baseTimeLeft - timePassed);
            } else {
                setTimeLeft(baseTimeLeft);
            }
        },
    );
    useEffect(() => {
        initializeNewTimeLeft(baseTimeLeft, clocks.lastUpdated, isTicking);
    }, [baseTimeLeft, clocks.lastUpdated, isTicking]);

    useEffect(() => {
        if (!isTicking) return;

        const interval = setInterval(
            () => {
                const timePassed = new Date().valueOf() - clocks.lastUpdated;
                setTimeLeft(baseTimeLeft - timePassed);
            },
            isInTimeTrouble ? 100 : 1000,
        );
        return () => {
            clearInterval(interval);
        };
    }, [isTicking, isInTimeTrouble, baseTimeLeft, clocks.lastUpdated]);

    useEffect(() => {
        if (
            clocks.isFrozen ||
            !isInTimeTrouble ||
            playedWarningSoundRef.current ||
            viewer.playerColor !== color
        )
            return;

        AudioPlayer.playAudio(AudioType.LOW_TIME);
        playedWarningSoundRef.current = true;
    }, [timeLeft, color, viewer.playerColor, clocks.isFrozen, isInTimeTrouble]);

    const minutes = Math.max(0, Math.floor(timeLeft / 60000));
    const seconds = Math.max(0, (timeLeft % 60000) / 1000);

    const strMinutes = minutes.toString().padStart(2, "0");

    const strSeconds = isInTimeTrouble
        ? seconds.toFixed(2).padStart(5, "0") // xx.yy
        : Math.floor(seconds).toString().padStart(2, "0"); // xx

    return (
        <span
            className={clsx(
                "font-mono text-2xl",
                isInTimeTrouble && isTicking && "animate-freakout",
                seconds <= 0 &&
                    minutes <= 0 &&
                    clocks.isFrozen &&
                    "text-red-600",
            )}
        >
            {strMinutes}:{strSeconds}
        </span>
    );
};
export default GameClock;
