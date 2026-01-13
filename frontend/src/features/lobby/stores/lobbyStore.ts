import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import { OngoingGame, OpenSeek, PoolKeyStr } from "../lib/types";
import OpenSeekTracker from "../lib/openSeekTracker";
import { PoolKey } from "@/lib/apiClient";

interface LobbyStore {
    seeks: Set<PoolKeyStr>;
    requestedOpenSeek: boolean;
    ongoingGames: Map<string, OngoingGame>;
    openSeekTracker: OpenSeekTracker;
    lastSeekingPath: string | null;

    clearSeeks(): void;

    addSeek(pool: PoolKeyStr): void;
    removeSeek(pool: PoolKeyStr): void;

    setRequestedOpenSeek(isRequesting: boolean): void;

    addOngoingGames(games: OngoingGame[]): void;
    removeOngoingGame(gameToken: string): void;

    addOpenSeeks(newOpenSeek: OpenSeek[]): void;
    removeOpenSeek(userId: string, pool: PoolKey): void;

    setLastSeekingPath(path: string): void;
}

enableMapSet();
const useLobbyStore = createWithEqualityFn<LobbyStore>()(
    immer((set) => ({
        seeks: new Set(),
        requestedOpenSeek: false,
        ongoingGames: new Map(),
        openSeekTracker: new OpenSeekTracker(),
        lastSeekingPath: null,

        clearSeeks() {
            set((state) => {
                state.seeks.clear();
                state.openSeekTracker.clear();
            });
        },

        addSeek(pool) {
            set((state) => {
                state.seeks.add(pool);
            });
        },
        removeSeek(pool) {
            set((state) => {
                state.seeks.delete(pool);
            });
        },

        setRequestedOpenSeek(isRequesting) {
            set((state) => {
                state.requestedOpenSeek = isRequesting;
            });
        },

        addOngoingGames(games) {
            set((state) => {
                for (const game of games) {
                    state.ongoingGames.set(game.gameToken, game);
                }
            });
        },
        removeOngoingGame(gameToken) {
            set((state) => {
                state.ongoingGames.delete(gameToken);
            });
        },

        addOpenSeeks(newOpenSeek) {
            set((state) => {
                state.openSeekTracker.addSeeks(newOpenSeek);
            });
        },
        removeOpenSeek(userId, pool) {
            set((state) => {
                state.openSeekTracker.removeSeek(userId, pool);
            });
        },

        setLastSeekingPath(path) {
            set((state) => {
                state.lastSeekingPath = path;
            });
        },
    })),
    shallow,
);
export default useLobbyStore;
