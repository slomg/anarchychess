import EventBus from "../eventBus";

describe("EventBus", () => {
    let bus: EventBus<[number], number>;

    beforeEach(() => {
        bus = new EventBus();
    });

    describe("subscribe", () => {
        it("should call a listener with the correct argument", async () => {
            const listener = vi.fn((x: number) => x + 1);
            bus.subscribe(listener);

            const results = await bus.emit(5);

            expect(listener).toHaveBeenCalledWith(5);
            expect(results).toEqual([6]);
        });

        it("should not call a listener added mid emission", async () => {
            const listener1 = async (x: number) => {
                bus.subscribe(listener2);
                return x + 1;
            };
            const listener2 = (x: number) => {
                return x + 2;
            };

            bus.subscribe(listener1);

            const results = await bus.emit(0);

            expect(results).toEqual([1]);

            const nextResults = await bus.emit(0);
            expect(nextResults).toEqual([1, 2]);
        });
    });

    describe("unsubscribe", () => {
        it("should remove a listener", async () => {
            const listener = vi.fn((x: number) => x * 2);
            bus.subscribe(listener);
            bus.unsubscribe(listener);

            const results = await bus.emit(5);
            expect(listener).not.toHaveBeenCalled();
            expect(results).toEqual([]);
        });

        it("should be safe to unsubscribe a listener that was never subscribed", async () => {
            const listener = vi.fn();
            expect(() => bus.unsubscribe(listener)).not.toThrow();
        });
    });

    describe("emit", () => {
        it("should call all listeners and return their results in order", async () => {
            const listener1 = vi.fn((x: number) => x + 1);
            const listener2 = vi.fn(async (x: number) => x + 2);

            bus.subscribe(listener1);
            bus.subscribe(listener2);

            const results = await bus.emit(10);

            expect(listener1).toHaveBeenCalledWith(10);
            expect(listener2).toHaveBeenCalledWith(10);
            expect(results).toEqual([11, 12]);
        });
    });

    describe("emitUntilTruthy", () => {
        it("should return the first truthy result and stop", async () => {
            const listener1 = vi.fn(() => 0);
            const listener2 = vi.fn(() => 100);
            const listener3 = vi.fn(() => 200);

            bus.subscribe(listener1);
            bus.subscribe(listener2);
            bus.subscribe(listener3);

            const result = await bus.emitUntilTruthy(10);
            expect(result).toBe(100);
            expect(listener3).not.toHaveBeenCalled();
        });

        it("should return undefined if no listener returns truthy", async () => {
            const listener = vi.fn(() => 0);
            bus.subscribe(listener);

            const result = await bus.emitUntilTruthy(5);
            expect(result).toBeUndefined();
        });
    });
});
