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
            challenge,
        });
    });

    describe("isExpired", () => {
        it("should be false if challenge has not expired", () => {
            challenge.expiresAt = new Date(Date.now() + 60000).toISOString();
            store = createChallengeStore({
                challenge,
            });

            expect(store.getState().isExpired).toBe(false);
        });

        it("should be true if challenge has expired", () => {
            challenge.expiresAt = new Date(Date.now() - 100).toISOString();
            store = createChallengeStore({
                challenge,
            });

            expect(store.getState().isExpired).toBe(true);
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

    describe("setExpired", () => {
        it("should set isExpired to true", () => {
            store.getState().setExpired();
            expect(store.getState().isExpired).toBe(true);
        });
    });
});
