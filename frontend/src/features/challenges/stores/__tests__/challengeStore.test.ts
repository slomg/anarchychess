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

        it("should set isExpired to false if the new challenge has not expired", () => {
            const now = Date.now();
            const newChallenge = createFakeChallengeRequest({
                expiresAt: new Date(now + 60000).toISOString(),
            });
            store.setState({ isExpired: true });

            store.getState().setChallenge(newChallenge);

            expect(store.getState().isExpired).toBe(false);
        });

        it("should set isExpired to true if the new challenge has already expired", () => {
            const now = Date.now();
            const expiredChallenge = createFakeChallengeRequest({
                expiresAt: new Date(now - 10000).toISOString(),
            });
            store.setState({ isExpired: false });

            store.getState().setChallenge(expiredChallenge);

            expect(store.getState().isExpired).toBe(true);
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
