import useLiveChessStore from "@/features/liveGame/hooks/useLiveChessStore";
import LiveBotControls from "./LiveBotControls";
import BotOverControls from "./BotOverControls";
import { BotType } from "@/lib/apiClient";
import Card from "@/components/ui/Card";

const BotGameControlsCard = ({ botType }: { botType: BotType }) => {
    const { resultData, viewer } = useLiveChessStore((state) => ({
        viewer: state.viewer,
        resultData: state.resultData,
    }));

    const controlsComponent =
        viewer.playerColor === null || resultData !== null ? (
            <BotOverControls botType={botType} />
        ) : (
            <LiveBotControls />
        );

    return (
        <Card className="flex-row justify-center gap-2">
            {controlsComponent}
        </Card>
    );
};
export default BotGameControlsCard;
