import { ArrowUturnLeftIcon } from "@heroicons/react/24/outline";
import { useEffect, useEffectEvent, useRef } from "react";

import MoveHistoryToolbar from "@/features/chessboard/components/moveHistory/MoveHistoryToolbar";
import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import FlipButton from "@/features/chessboard/components/moveHistory/FlipButton";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { PositionId } from "@/features/chessboard/lib/position";
import { getNextLegalMoves } from "@/lib/apiClient";
import { AnalysisPageType } from "./AnalysisSide";
import Button from "@/components/ui/Button";

const SetupAnalysisPositionPage = ({
    setSelectedPage,
}: {
    setSelectedPage: (page: AnalysisPageType) => void;
}) => {
    const { positionHistory, setSetupMode, setLatestLegalMoves } =
        useChessboardStore((x) => ({
            positionHistory: x.positionHistory,
            setSetupMode: x.setSetupMode,
            setLatestLegalMoves: x.setLatestLegalMoves,
        }));

    const rootIdBeforeRef = useRef<PositionId | null>(null);
    const updateIdBeforeEvent = useEffectEvent(
        () => (rootIdBeforeRef.current = positionHistory.root.positionId),
    );
    useEffect(() => {
        setSetupMode(true);
        updateIdBeforeEvent();
        return () => {
            setSetupMode(false);
        };
    }, [setSetupMode]);

    async function goBack(): Promise<void> {
        if (positionHistory.root.positionId === rootIdBeforeRef.current) {
            setSelectedPage(AnalysisPageType.Main);
            return;
        }

        const { error, data: movePaths } = await getNextLegalMoves({
            query: {
                fen: positionHistory.root.fen,
            },
        });
        if (error || movePaths === undefined) {
            console.error(
                "SetupAnalysisPositionPage goBack getNextLegalMoves",
                error,
            );
            return;
        }

        const legalMoves = decodeMovePathIntoLegalMoves(movePaths);
        setLatestLegalMoves(legalMoves);
        setSelectedPage(AnalysisPageType.Main);
    }

    return (
        <>
            <MoveHistoryToolbar
                className="order-1 lg:order-2"
                leftActions={
                    <Button title="Go Back" onClick={goBack}>
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
