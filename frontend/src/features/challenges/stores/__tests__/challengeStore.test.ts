import { StoreApi } from "zustand";
import { ChallengeStore, createChallengeStore } from "../challengeStore";
import { ChallengeRequest } from "@/lib/apiClient";
import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";

describe("challengeStore", () => {
    let store: StoreApi<ChallengeStore>;
    let challenge: ChallengeRequest;

    beforeEach(() => {
        challenge = createFakeChallengeRequest();
        store = createChallengeStore({
            challenge: challenge,
        });
    });

    describe("isExpired", () => {
        it("should return false when not expired", async () => {
            const now = Date.now();
            vi.setSystemTime(now);
            challenge.expiresAt = new Date(now + 60000).toISOString();
            store.setState({ challenge });

            expect(store.getState().isExpired()).toBe(false);
        });

        it("should return true after it expires", () => {
            const now = Date.now();
            vi.setSystemTime(now);
            challenge.expiresAt = new Date(now + 60000).toISOString();
            store.setState({ challenge });

            expect(store.getState().isExpired()).toBe(false);

            vi.setSystemTime(now + 61000);

            expect(store.getState().isExpired()).toBe(true);
        });
    });
});
