import { useRouter } from "next/navigation";

import useLiveChessStore from "@/features/liveGame/hooks/useLiveChessStore";
import GameOverPopup from "@/features/liveGame/components/GameOverPopup";
import { invertColor } from "@/lib/utils/chessUtils";
import useBotMatch from "../hooks/useBotMatch";
import Button from "@/components/ui/Button";
import constants from "@/lib/constants";

const BotGameOverPopup = () => {
    const viewer = useLiveChessStore((x) => x.viewer);

    const router = useRouter();
    const { matchBotGame, isMatching } = useBotMatch();

    function playNewBot() {
        router.push(constants.PATHS.BOT);
    }

    async function startNewGame() {
        const myColor =
            viewer.playerColor !== null
                ? invertColor(viewer.playerColor)
                : null;
        await matchBotGame(myColor);
    }

    return (
        <GameOverPopup
            controls={
                viewer.playerColor !== null ? (
                    <>
                        <Button className="flex-1" onClick={playNewBot}>
                            PLAY NEW BOT
                        </Button>
                        <Button
                            className="flex-1"
                            onClick={startNewGame}
                            disabled={isMatching}
                        >
                            REMATCH
                        </Button>
                    </>
                ) : (
                    <Button
                        className="flex-1"
                        onClick={startNewGame}
                        disabled={isMatching}
                    >
                        PLAY ANARCHY BOT
                    </Button>
                )
            }
        />
    );
};
export default BotGameOverPopup;
