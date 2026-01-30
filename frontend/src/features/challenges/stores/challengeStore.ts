import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";

import { ChallengeRequest } from "@/lib/apiClient";

export interface ChallengeStoreProps {
    challenge: ChallengeRequest;
}

export interface ChallengeStore {
    challenge: ChallengeRequest;

    isExpired(): boolean;
    setChallenge(challenge: ChallengeRequest): void;
    setCancelled(cancelledBy: string | null): void;
}

export function createChallengeStore(initState: ChallengeStoreProps) {
    return createWithEqualityFn<ChallengeStore>()(
        immer((set, get) => ({
            ...initState,
            isExpired() {
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
        })),
        shallow,
    );
}
