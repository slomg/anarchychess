import { StateCreator } from "zustand";

import type { LiveChessStore } from "./liveChessStore";
import { PendingOvertimeRemoval } from "../lib/types";
import { GameColor } from "@/lib/apiClient";

export interface OvertimeSliceProps {
    whiteOvertime: PendingOvertimeRemoval[] | null;
    blackOvertime: PendingOvertimeRemoval[] | null;
}

export interface OvertimeSlice {
    whiteOvertime: PendingOvertimeRemoval[] | null;
    blackOvertime: PendingOvertimeRemoval[] | null;

    setOvertime(
        overtimedPlayer: GameColor,
        playerOvertime: PendingOvertimeRemoval[],
    ): void;
    clearOvertime(): void;
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

        setOvertime(overtimedPlayer, playerOvertime) {
            set((state) => {
                if (overtimedPlayer === GameColor.WHITE) {
                    state.whiteOvertime = playerOvertime;
                } else {
                    state.blackOvertime = playerOvertime;
                }
            });
        },

        clearOvertime() {
            set((state) => {
                state.whiteOvertime = null;
                state.blackOvertime = null;
            });
        },
    });
}
