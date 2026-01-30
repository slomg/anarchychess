import { StoreApi } from "zustand";

import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import { ChallengeStore, createChallengeStore } from "../challengeStore";
import { ChallengeRequest } from "@/lib/apiClient";

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

    describe("setChallenge", () => {
        it("should update the challenge in the store", () => {
            const newChallenge = createFakeChallengeRequest();
            store.getState().setChallenge(newChallenge);

            expect(store.getState().challenge).toEqual(newChallenge);
        });
    });

    describe("setCancelled", () => {
        it("should update the cancelledBy property of the challenge", () => {
            store.getState().setCancelled("user123");

            expect(store.getState().challenge.cancelledBy).toBe("user123");
        });
    });
});
