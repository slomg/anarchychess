import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import {
    createGamePlaySlice,
    GamePlaySlice,
    GamePlaySliceProps,
} from "./gamePlaySlice";
import {
    createGameStateSlice,
    GameStateSlice,
    GameStateSliceProps,
} from "./gameStateSlice";
import {
    createOvertimeSlice,
    OvertimeSlice,
    OvertimeSliceProps,
} from "./overtimeSlice";

import { createRematchSlice, RematchSlice } from "./rematchSlice";

export type LiveChessStoreProps = GamePlaySliceProps &
    GameStateSliceProps &
    OvertimeSliceProps;
export type LiveChessStore = GamePlaySlice &
    GameStateSlice &
    RematchSlice &
    OvertimeSlice;

enableMapSet();
export default function createLiveChessStore(initState: LiveChessStoreProps) {
    return createWithEqualityFn<LiveChessStore>()(
        immer((...a) => ({
            ...createGamePlaySlice(initState)(...a),
            ...createGameStateSlice(initState)(...a),
            ...createRematchSlice(...a),
            ...createOvertimeSlice(initState)(...a),
        })),
        shallow,
    );
}
