import { MaybePromise } from "@/types/types";
import AsyncLock from "./asyncLock";

export type EventListener<TArgs extends unknown[], TResult = void> = (
    ...args: TArgs
) => MaybePromise<TResult>;

export default class EventBus<TArgs extends unknown[], TResult = void> {
    public readonly listeners: Set<EventListener<TArgs, TResult>> = new Set();
    _lock = new AsyncLock();

    subscribe(fn: EventListener<TArgs, TResult>): Promise<void> {
        return this._lock.acquire(() => {
            this.listeners.add(fn);
        });
    }

    unsubscribe(fn: EventListener<TArgs, TResult>): Promise<void> {
        return this._lock.acquire(() => {
            this.listeners.delete(fn);
        });
    }

    emit(...args: TArgs): Promise<TResult[]> {
        return this._lock.acquire(async () => {
            const promises: MaybePromise<TResult>[] = [];
            for (const listener of this.listeners) {
                promises.push(listener(...args));
            }
            const results = await Promise.all(promises);
            return results;
        });
    }

    emitUntilTruthy(...args: TArgs): Promise<TResult | undefined> {
        return this._lock.acquire(async () => {
            for (const listener of this.listeners) {
                const result = await listener(...args);
                if (result) return result;
            }
        });
    }
}
