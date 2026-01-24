import { StateCreator } from "zustand";

import type { LiveChessStore } from "./liveChessStore";
import { PlayerOvertime } from "../lib/types";
import { GameColor } from "@/lib/apiClient";

export interface OvertimeSliceProps {
    overtimeTurnStartedAt: number;
    whiteOvertime: PlayerOvertime | null;
    blackOvertime: PlayerOvertime | null;
}

export interface OvertimeSlice {
    overtimeTurnStartedAt: number;
    whiteOvertime: PlayerOvertime | null;
    blackOvertime: PlayerOvertime | null;

    setOvertime(
        overtimedPlayer: GameColor,
        playerOvertime: PlayerOvertime,
        overtimeTurnStartedAt: number,
    ): void;
}

export function createOvertimeSlice(
    initState: OvertimeSliceProps,
): StateCreator<
    LiveChessStore,
    [["zustand/immer", never], never],
    [],
    OvertimeSlice
> {
    return (set) => ({
        ...initState,

        setOvertime(overtimedPlayer, playerOvertime, overtimeTurnStartedAt) {
            set((state) => {
                state.overtimeTurnStartedAt = overtimeTurnStartedAt;

                if (overtimedPlayer === GameColor.WHITE) {
                    state.whiteOvertime = playerOvertime;
                } else {
                    state.blackOvertime = playerOvertime;
                }
            });
        },
    });
}
