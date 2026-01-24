import { StateCreator } from "zustand";

import type { LiveChessStore } from "./liveChessStore";
import { PlayerOvertime } from "../lib/types";

export interface OvertimeSliceProps {
    whiteOvertime: PlayerOvertime | null;
    blackOvertime: PlayerOvertime | null;
}

export interface OvertimeSlice {
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
