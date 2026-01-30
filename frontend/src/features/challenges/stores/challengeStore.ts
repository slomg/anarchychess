import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";

import { ChallengeRequest } from "@/lib/apiClient";

export interface ChallengeStoreProps {
    challenge: ChallengeRequest;
}

export interface ChallengeStore {
    challenge: ChallengeRequest;
    isExpired: boolean;

    setChallenge(challenge: ChallengeRequest): void;
    setCancelled(cancelledBy: string): void;
    setExpired(): void;
}

export function createChallengeStore(initState: ChallengeStoreProps) {
    return createWithEqualityFn<ChallengeStore>()(
        immer((set) => ({
            ...initState,
            isExpired:
                new Date().getTime() >=
                new Date(initState.challenge.expiresAt).getTime(),

            setChallenge(challenge) {
                set((state) => {
                    state.challenge = challenge;
                    state.isExpired =
                        new Date().getTime() >=
                        new Date(challenge.expiresAt).getTime();
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
