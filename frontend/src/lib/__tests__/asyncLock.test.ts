import AsyncLock from "../asyncLock";

describe("AsyncLock", () => {
    let lock: AsyncLock;

    beforeEach(() => {
        lock = new AsyncLock();
    });

    it("should run functions sequentially", async () => {
        const order: number[] = [];

        await Promise.all([
            lock.acquire(async () => {
                await new Promise((r) => setTimeout(r, 30));
                order.push(1);
            }),
            lock.acquire(async () => {
                await new Promise((r) => setTimeout(r, 10));
                order.push(2);
            }),
            lock.acquire(async () => {
                order.push(3);
            }),
        ]);

        expect(order).toEqual([1, 2, 3]);
    });

    it("should not allow concurrent execution", async () => {
        const lock = new AsyncLock();
        let active = 0;
        let maxActive = 0;

        await Promise.all([
            lock.acquire(async () => {
                active++;
                maxActive = Math.max(maxActive, active);
                await new Promise((r) => setTimeout(r, 20));
                active--;
            }),
            lock.acquire(async () => {
                active++;
                maxActive = Math.max(maxActive, active);
                await new Promise((r) => setTimeout(r, 20));
                active--;
            }),
            lock.acquire(async () => {
                active++;
                maxActive = Math.max(maxActive, active);
                await new Promise((r) => setTimeout(r, 20));
                active--;
            }),
        ]);

        expect(maxActive).toBe(1);
    });

    it("should release the lock when the function throws", async () => {
        const lock = new AsyncLock();
        const order: string[] = [];

        await expect(
            lock.acquire(async () => {
                order.push("first");
                throw new Error("fail");
            }),
        ).rejects.toThrow();

        await lock.acquire(async () => {
            order.push("second");
        });

        expect(order).toEqual(["first", "second"]);
    });

    it("should preserve first in first out order", async () => {
        const lock = new AsyncLock();
        const order: number[] = [];

        const tasks = Array.from({ length: 5 }, (_, i) =>
            lock.acquire(async () => {
                await new Promise((r) => setTimeout(r, 5));
                order.push(i);
            }),
        );

        await Promise.all(tasks);

        expect(order).toEqual([0, 1, 2, 3, 4]);
    });
});
