import { StateCreator } from "zustand";

import {
    Clocks,
    DrawState,
    GameColor,
    GamePlayer,
    GameResultData,
    PoolKey,
} from "@/lib/apiClient";

import type { LiveChessStore, LiveChessStoreProps } from "./liveChessStore";

export interface GameStateSliceProps {
    gameToken: string;

    whitePlayer: GamePlayer;
    blackPlayer: GamePlayer;

    sourceRevision: number;
    pool: PoolKey | null;

    resultData: GameResultData | null;
    drawState: DrawState | null;
}

export interface GameStateSlice extends GameStateSliceProps {
    getPlayerByColor(color: GameColor): GamePlayer;
    decrementDrawCooldown(): void;
    drawStateChange(drawState: DrawState): void;
    endGame(
        plyNumber: number,
        resultData: GameResultData,
        finalClocks?: Clocks,
    ): void;

    resetState(initState: LiveChessStoreProps): void;
}

export function createGameStateSlice(
    initState: GameStateSliceProps,
): StateCreator<
    LiveChessStore,
    [["zustand/immer", never], never],
    [],
    GameStateSlice
> {
    return (set, get, store) => ({
        ...initState,

        getPlayerByColor(color) {
            const { whitePlayer, blackPlayer } = get();
            return color === GameColor.WHITE ? whitePlayer : blackPlayer;
        },

        decrementDrawCooldown() {
            set((state) => {
                if (state.drawState !== null) {
                    state.drawState.whiteCooldown = Math.max(
                        0,
                        state.drawState.whiteCooldown - 1,
                    );
                    state.drawState.blackCooldown = Math.max(
                        0,
                        state.drawState.blackCooldown - 1,
                    );
                }
            });
        },

        drawStateChange(drawState) {
            set((state) => {
                state.drawState = drawState;
            });
        },

        endGame(plyNumber, resultData, finalClocks) {
            const { setClocks } = get();

            if (finalClocks) {
                setClocks(plyNumber, finalClocks);
            }
            set((state) => {
                if (state.whitePlayer.rating && resultData.whiteRatingChange)
                    state.whitePlayer.rating += resultData.whiteRatingChange;
                if (state.blackPlayer.rating && resultData.blackRatingChange)
                    state.blackPlayer.rating += resultData.blackRatingChange;

                state.resultData = resultData;
            });
        },

        resetState(initState) {
            set(() => ({ ...store.getInitialState(), ...initState }));
        },
    });
}
