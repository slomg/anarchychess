import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";

import { ChallengeRequest } from "@/lib/apiClient";

export interface ChallengeStoreProps {
    challenge: ChallengeRequest;
}

export interface ChallengeStore {
    challenge: ChallengeRequest;
    readonly isExpired: boolean;

    setChallenge(challenge: ChallengeRequest): void;
    setCancelled(cancelledBy: string | null): void;
    setExpired(): void;
}

export function createChallengeStore(initState: ChallengeStoreProps) {
    return createWithEqualityFn<ChallengeStore>()(
        immer((set, get) => ({
            ...initState,
            get isExpired() {
                return (
                    new Date().getTime() >=
                    new Date(get().challenge.expiresAt).getTime()
                );
            },

            setChallenge(challenge) {
                set((state) => {
                    state.challenge = challenge;
                });
            },
            setCancelled(cancelledBy) {
                set((state) => {
                    state.challenge.cancelledBy = cancelledBy;
                });
            },
            setExpired() {
                set((state) => {
                    state.isExpired = true;
                });
            },
        })),
        shallow,
    );
}
