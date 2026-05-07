import { StateCreator } from "zustand";

import type { ChessboardStore } from "./chessboardStore";
import { LogicalPoint } from "@/features/point/types";
import EventBus from "@/lib/eventBus";

export interface SetupModeMoveEvent {
    from: LogicalPoint;
    to: LogicalPoint;
}

export interface SetupModeSlice {
    isSetupMode: boolean;
    setupModeMoveEvent: EventBus<[event: SetupModeMoveEvent], void>;

    setSetupMode(setupMode: boolean): void;
}

export const createSetupModeSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    SetupModeSlice
> = (set) => ({
    isSetupMode: false,
    setupModeMoveEvent: new EventBus(),

    setSetupMode(setupMode) {
        set((state) => {
            state.isSetupMode = setupMode;
        });
    },
});
