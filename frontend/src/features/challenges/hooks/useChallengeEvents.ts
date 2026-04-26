import { useRouter } from "next/navigation";
import { StoreApi } from "zustand";

import gameStartRedirect from "@/features/liveGame/lib/gameStartRedirect";
import { useChallengeInstanceEvent } from "./useChallengeHub";
import { ChallengeStore } from "../stores/challengeStore";

export default function useChallengeEvents(
    challengeStore: StoreApi<ChallengeStore>,
    challengeToken: string,
) {
    const router = useRouter();

    useChallengeInstanceEvent(
        challengeToken,
        "ChallengeAcceptedAsync",
        async (gameToken, challengeToken) => {
            const currentChallengeToken =
                challengeStore.getState().challenge.challengeToken;
            if (challengeToken !== currentChallengeToken) {
                return;
            }

            await gameStartRedirect(gameToken, router);
        },
    );

    useChallengeInstanceEvent(
        challengeToken,
        "ChallengeCancelledAsync",
        (cancelledBy, challengeToken) => {
            const { challenge: currentChallenge, setCancelled } =
                challengeStore.getState();
            if (challengeToken !== currentChallenge.challengeToken) {
                return;
            }
            setCancelled(cancelledBy);
        },
    );

    useChallengeInstanceEvent(
        challengeToken,
        "ReceiveUpdatedChallengeAsync",
        async (challenge) => {
            const { challenge: currentChallenge, setChallenge } =
                challengeStore.getState();
            if (challenge.challengeToken !== currentChallenge.challengeToken) {
                return;
            }

            setChallenge(challenge);
            if (challenge.resolvedGame) {
                await gameStartRedirect(challenge.resolvedGame, router);
            }
        },
    );
}
