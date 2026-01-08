import { immerable } from "immer";

import { poolKeyToStr } from "./matchmakingKeys";
import { OpenSeek, PoolKeyStr } from "./types";
import { PoolKey } from "@/lib/apiClient";

export default class OpenSeekTracker {
    [immerable] = true;

    _userOpenSeeks = new Map<string, Map<PoolKeyStr, OpenSeek>>();

    _interleavedOpenSeeks: OpenSeek[] = [];

    get interleavedOpenSeeks(): readonly OpenSeek[] {
        return this._interleavedOpenSeeks;
    }

    addSeeks(newOpenSeek: OpenSeek[]): void {
        for (const openSeek of newOpenSeek) {
            const byPool =
                this._userOpenSeeks.get(openSeek.userId) ??
                new Map<PoolKeyStr, OpenSeek>();
            byPool.set(poolKeyToStr(openSeek.pool), openSeek);
            this._userOpenSeeks.set(openSeek.userId, byPool);
        }

        this._calculateInterleavedOpenSeeks();
    }

    removeSeek(userId: string, pool: PoolKey): void {
        const byPool = this._userOpenSeeks.get(userId);
        if (!byPool) return;

        byPool.delete(poolKeyToStr(pool));
        if (byPool.size === 0) {
            this._userOpenSeeks.delete(userId);
        }

        this._calculateInterleavedOpenSeeks();
    }

    clear(): void {
        this._userOpenSeeks = new Map();
        this._interleavedOpenSeeks = [];
    }

    _calculateInterleavedOpenSeeks() {
        const userOpenSeeksList: OpenSeek[][] = [];
        let maxLength = 0;
        for (const userPools of this._userOpenSeeks.values()) {
            const userOpenSeeks = [...userPools.values()];
            userOpenSeeksList.push(userOpenSeeks);
            maxLength = Math.max(userOpenSeeks.length, maxLength);
        }

        const result: OpenSeek[] = [];
        for (let i = 0; i < maxLength; i++) {
            for (const openSeeks of userOpenSeeksList) {
                if (i < openSeeks.length) {
                    result.push(openSeeks[i]);
                }
            }
        }

        this._interleavedOpenSeeks = result;
    }
}
