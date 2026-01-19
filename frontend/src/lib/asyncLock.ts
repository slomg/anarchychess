export default class AsyncLock {
    _locked = false;
    _queue: (() => void)[] = [];

    async acquire<T>(fn: () => Promise<T> | T): Promise<T> {
        await new Promise<void>((resolve) => {
            if (this._locked) {
                this._queue.push(resolve);
            } else {
                this._locked = true;
                resolve();
            }
        });

        try {
            return await fn();
        } finally {
            const next = this._queue.shift();
            if (next) {
                next();
            } else {
                this._locked = false;
            }
        }
    }
}
