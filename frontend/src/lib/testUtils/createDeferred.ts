export default function createDeferred<T = void>(): {
    promise: Promise<T>;
    resolve: (value: T) => void;
    reject: (reason?: unknown) => void;
} {
    let resolve!: (value: T) => void;
    let reject!: (reason?: unknown) => void;
    const promise = new Promise<T>((r, j) => {
        resolve = r;
        reject = j;
    });
    return { promise, resolve, reject };
}
