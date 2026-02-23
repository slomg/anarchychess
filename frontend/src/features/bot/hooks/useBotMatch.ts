import { checkBotHealth, GameColor, startBotGame } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { randomizeColor } from "@/lib/utils/chessUtils";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function useBotMatch(): {
    matchBotGame: (color: GameColor | null) => Promise<boolean>;
    isMatching: boolean;
} {
    const router = useRouter();
    const [isMatching, setIsMatching] = useState(false);

    async function matchBotGame(color: GameColor | null): Promise<boolean> {
        setIsMatching(true);

        try {
            const isHealthy = await redirectIfUnhealthy();
            if (!isHealthy) {
                return false;
            }

            const myColor = color ?? randomizeColor();
            const { error, data: gameToken } = await startBotGame({
                query: { myColor },
            });

            if (error || !gameToken) {
                console.error("useBotMatch matchBotGame", error);
                return false;
            }

            router.push(`${constants.PATHS.BOT}/${gameToken}`);
            return true;
        } finally {
            setIsMatching(false);
        }
    }

    async function redirectIfUnhealthy(): Promise<boolean> {
        const { error, data: isHealthy } = await checkBotHealth();
        if (error || isHealthy === undefined) {
            console.error("useBotMatch redirectIfUnhealthy", error);
            return false;
        }

        if (!isHealthy) {
            router.push(constants.PATHS.BOT_OFFLINE);
        }

        return isHealthy;
    }

    return { matchBotGame, isMatching };
}
