"use client";

import useLiveChessStore from "../../hooks/useLiveChessStore";
import LiveGameControls from "./LiveGameControls";
import GameOverControls from "./GameOverControls";
import Card from "@/components/ui/Card";

const GameControlsCard = () => {
    const { resultData, viewer } = useLiveChessStore((state) => ({
        viewer: state.viewer,
        resultData: state.resultData,
    }));

    const controlsComponent =
        viewer.playerColor === null || resultData ? (
            <GameOverControls />
        ) : (
            <LiveGameControls />
        );

    return (
        <Card className="flex-row justify-center gap-2">
            {controlsComponent}
        </Card>
    );
};
export default GameControlsCard;
