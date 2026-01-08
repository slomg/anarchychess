import { SeekKeyStr, PoolKeyStr } from "./types";
import { PoolKey } from "@/lib/apiClient";

export function OpenSeekToKeyStr(userId: string, pool: PoolKey): SeekKeyStr {
    return `${userId}:${poolKeyToStr(pool)}`;
}

export function poolKeyToStr(poolKey: PoolKey): PoolKeyStr {
    return `${poolKey.poolType}-${poolKey.timeControl.baseSeconds}+${poolKey.timeControl.incrementSeconds}`;
}
