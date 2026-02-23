import { FlagIcon } from "@heroicons/react/24/solid";

import GameControlButton from "@/features/liveGame/components/GameControls/GameControlButton";
import { useBotEmitter } from "../../hooks/useBotHub";
import useLiveChessStore from "@/features/liveGame/hooks/useLiveChessStore";

const LiveBotControls = () => {
    const gameToken = useLiveChessStore((x) => x.gameToken);
    const sendBotEvent = useBotEmitter(gameToken);

    const endGame = () => sendBotEvent("ResignAsync", gameToken);

    return (
        <>
            <GameControlButton
                icon={FlagIcon}
                title="Resign"
                onClick={endGame}
                needsConfirmation
            />
        </>
    );
};
export default LiveBotControls;
