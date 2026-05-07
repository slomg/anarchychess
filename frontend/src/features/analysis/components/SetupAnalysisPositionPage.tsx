import { ArrowUturnLeftIcon } from "@heroicons/react/24/outline";
import { useEffect } from "react";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { AnalysisPageType } from "./AnalysisSide";
import MoveHistoryToolbar from "@/features/chessboard/components/moveHistory/MoveHistoryToolbar";
import Button from "@/components/ui/Button";
import FlipButton from "@/features/chessboard/components/moveHistory/FlipButton";

const SetupAnalysisPositionPage = ({
    setSelectedPage,
}: {
    setSelectedPage: (page: AnalysisPageType) => void;
}) => {
    const setSetupMode = useChessboardStore((x) => x.setSetupMode);

    return (
        <>
            <MoveHistoryToolbar
                className="order-1 lg:order-2"
                leftActions={
                    <Button
                        title="Go Back"
                        onClick={() => setSelectedPage(AnalysisPageType.Main)}
                    >
                        <ArrowUturnLeftIcon className="h-8 w-8" />
                    </Button>
                }
                rightActions={<FlipButton />}
            />

            <div
                className="order-2 flex-1 lg:order-1"
                data-testid="analysisPositionSetup"
            ></div>
        </>
    );
};
export default SetupAnalysisPositionPage;
