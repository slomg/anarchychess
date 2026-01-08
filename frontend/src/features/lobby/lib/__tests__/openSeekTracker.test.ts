import createFakeOpenSeek from "@/lib/testUtils/fakers/openSeekFaker";
import OpenSeekTracker from "../openSeekTracker";
import { createFakePoolKey } from "../../../../lib/testUtils/fakers/poolKeyFaker";
import { PoolType } from "@/lib/apiClient";

describe("OpenSeekTracker", () => {
    let tracker: OpenSeekTracker;

    beforeEach(() => {
        tracker = new OpenSeekTracker();
    });

    describe("addSeeks", () => {
        it("should add a single open seek", () => {
            const seek = createFakeOpenSeek();

            tracker.addSeeks([seek]);

            expect(tracker.interleavedOpenSeeks).toEqual([seek]);
        });

        it("should add multiple open seeks for the same user", () => {
            const seek1 = createFakeOpenSeek({
                userId: "user1",
                pool: createFakePoolKey({ poolType: PoolType.CASUAL }),
            });
            const seek2 = createFakeOpenSeek({
                userId: "user1",
                pool: createFakePoolKey({ poolType: PoolType.RATED }),
            });

            tracker.addSeeks([seek1, seek2]);

            expect(tracker.interleavedOpenSeeks).toEqual([seek1, seek2]);
        });

        it("should add seeks for multiple users and interleave them", () => {
            const user1Seek1 = createFakeOpenSeek({
                userId: "user1",
                pool: createFakePoolKey({ poolType: PoolType.CASUAL }),
            });
            const user1Seek2 = createFakeOpenSeek({
                userId: "user1",
                pool: createFakePoolKey({ poolType: PoolType.RATED }),
            });
            const user2Seek1 = createFakeOpenSeek({
                userId: "user2",
                pool: createFakePoolKey({ poolType: PoolType.CASUAL }),
            });
            const user2Seek2 = createFakeOpenSeek({
                userId: "user2",
                pool: createFakePoolKey({ poolType: PoolType.RATED }),
            });

            tracker.addSeeks([user1Seek1, user2Seek1, user1Seek2, user2Seek2]);

            expect(tracker.interleavedOpenSeeks).toEqual([
                user1Seek1,
                user2Seek1,
                user1Seek2,
                user2Seek2,
            ]);
        });

        it("should update an existing seek if the same user and pool are added again", () => {
            const pool = createFakePoolKey();
            const seek1 = createFakeOpenSeek({
                userId: "user1",
                pool,
            });
            const seek2 = createFakeOpenSeek({
                userId: "user1",
                pool,
            });

            tracker.addSeeks([seek1]);
            tracker.addSeeks([seek2]);

            expect(tracker.interleavedOpenSeeks).toEqual([seek2]);
        });

        it("should handle empty input", () => {
            tracker.addSeeks([]);

            expect(tracker.interleavedOpenSeeks).toEqual([]);
        });
    });

    describe("removeSeek", () => {
        it("should remove a seek by user and pool", () => {
            const seek = createFakeOpenSeek({});
            tracker.addSeeks([seek]);

            tracker.removeSeek(seek.userId, seek.pool);

            expect(tracker.interleavedOpenSeeks).toEqual([]);
        });

        it("should remove only the specified pool and keep other pools for the user", () => {
            const seek1 = createFakeOpenSeek({
                userId: "user1",
                pool: createFakePoolKey({ poolType: PoolType.CASUAL }),
            });
            const seek2 = createFakeOpenSeek({
                userId: "user1",
                pool: createFakePoolKey({ poolType: PoolType.RATED }),
            });
            tracker.addSeeks([seek1, seek2]);

            tracker.removeSeek("user1", seek1.pool);

            expect(tracker.interleavedOpenSeeks).toEqual([seek2]);
        });

        it("should do nothing if the user does not exist", () => {
            const seek = createFakeOpenSeek();
            tracker.addSeeks([seek]);

            tracker.removeSeek("nonexistent", seek.pool);

            expect(tracker.interleavedOpenSeeks).toEqual([seek]);
        });

        it("should do nothing if the pool does not exist for the user", () => {
            const seek = createFakeOpenSeek({
                pool: createFakePoolKey({ poolType: PoolType.CASUAL }),
            });
            tracker.addSeeks([seek]);

            tracker.removeSeek(
                "user1",
                createFakePoolKey({ poolType: PoolType.RATED }),
            );

            expect(tracker.interleavedOpenSeeks).toEqual([seek]);
        });
    });

    describe("clear", () => {
        it("should remove all open seeks", () => {
            const seek1 = createFakeOpenSeek();
            const seek2 = createFakeOpenSeek();
            tracker.addSeeks([seek1, seek2]);
            expect(tracker.interleavedOpenSeeks).toEqual([seek1, seek2]);

            tracker.clear();

            expect(tracker.interleavedOpenSeeks).toEqual([]);
        });
    });
});
