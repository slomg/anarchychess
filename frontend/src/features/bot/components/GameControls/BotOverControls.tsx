import { ArrowPathIcon, PlusIcon } from "@heroicons/react/24/solid";
import { useRouter } from "next/navigation";

import GameControlButton from "@/features/liveGame/components/GameControls/GameControlButton";
import useLiveChessStore from "@/features/liveGame/hooks/useLiveChessStore";
import { invertColor } from "@/lib/utils/chessUtils";
import useBotMatch from "../../hooks/useBotMatch";
import constants from "@/lib/constants";

const BotOverControls = () => {
    const viewer = useLiveChessStore((x) => x.viewer);
    const router = useRouter();
    const { matchBotGame, isMatching } = useBotMatch();

    function playNewBot() {
        router.push(constants.PATHS.BOT);
    }

    async function rematch() {
        const myColor =
            viewer.playerColor !== null
                ? invertColor(viewer.playerColor)
                : null;
        await matchBotGame(myColor);
    }

    return (
        <>
            <GameControlButton
                icon={PlusIcon}
                onClick={playNewBot}
                data-testid="botOverControlsNewGame"
            >
                Play New Bot
            </GameControlButton>
            {viewer.playerColor !== null && (
                <GameControlButton
                    icon={ArrowPathIcon}
                    onClick={rematch}
                    disabled={isMatching}
                    data-testid="botOverControlsRematch"
                >
                    Rematch
                </GameControlButton>
            )}
        </>
    );
};
export default BotOverControls;
