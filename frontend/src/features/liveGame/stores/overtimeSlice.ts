import { StateCreator } from "zustand";

import type { LiveChessStore } from "./liveChessStore";
import { PlayerOvertime } from "../lib/types";

export interface OvertimeSliceProps {
    overtimeTurnStartedAt: number;
    whiteOvertime: PlayerOvertime | null;
    blackOvertime: PlayerOvertime | null;
}

export interface OvertimeSlice {
    overtimeTurnStartedAt: number;
    whiteOvertime: PlayerOvertime | null;
    blackOvertime: PlayerOvertime | null;
}

export function createOvertimeSlice(
    initState: OvertimeSliceProps,
): StateCreator<
    LiveChessStore,
    [["zustand/immer", never], never],
    [],
    OvertimeSlice
> {
    return () => ({
        ...initState,
    });
}
