import {
    ArrowPathIcon,
    ArrowUturnLeftIcon,
    TrashIcon,
} from "@heroicons/react/24/outline";

import { useEffect, useEffectEvent, useRef } from "react";

import MoveHistoryToolbar from "@/features/chessboard/components/moveHistory/MoveHistoryToolbar";
import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import FlipButton from "@/features/chessboard/components/moveHistory/FlipButton";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import SetupPositionPieceProperties from "./SetupPositionPieceProperties";
import { PositionId } from "@/features/chessboard/lib/position";
import { GameColor, getNextLegalMoves } from "@/lib/apiClient";
import SetupPositionPieces from "./SetupPositionPieces";
import { AnalysisPageType } from "./AnalysisSide";
import Selector from "@/components/ui/Selector";
import Button from "@/components/ui/Button";

const SetupAnalysisPositionPage = ({
    setSelectedPage,
}: {
    setSelectedPage: (page: AnalysisPageType) => void;
}) => {
    const {
        positionHistory,
        setSetupMode,
        setLatestLegalMoves,
        clearSetupModeBoard,
        resetSetupModeBoard,
        setSetupModeSideToMove,
    } = useChessboardStore((x) => ({
        positionHistory: x.positionHistory,
        setSetupMode: x.setSetupMode,
        setLatestLegalMoves: x.setLatestLegalMoves,
        clearSetupModeBoard: x.clearSetupModeBoard,
        resetSetupModeBoard: x.resetSetupModeBoard,
        setSetupModeSideToMove: x.setSetupModeSideToMove,
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
                rightActions={
                    <>
                        <Button
                            title="Reset Board"
                            onClick={resetSetupModeBoard}
                        >
                            <ArrowPathIcon className="h-8 w-8" />
                        </Button>
                        <Button
                            title="Clear Board"
                            onClick={clearSetupModeBoard}
                        >
                            <TrashIcon className="h-8 w-8" />
                        </Button>
                        <FlipButton />
                    </>
                }
            />

            <div
                className="order-2 flex flex-1 flex-col gap-5 overflow-auto p-4
                    lg:order-1"
                data-testid="analysisPositionSetup"
            >
                <SetupPositionPieces />

                <hr className="text-secondary/30" />

                <Selector
                    options={[
                        {
                            label: "White to Move",
                            value: GameColor.WHITE,
                        },
                        {
                            label: "Black to Move",
                            value: GameColor.BLACK,
                        },
                    ]}
                    onChange={(event) =>
                        setSetupModeSideToMove(event.target.value)
                    }
                    value={
                        positionHistory.viewingPosition?.sideToMove ??
                        positionHistory.root.sideToMove
                    }
                    data-testid="setupPositionSideToMove"
                />

                <SetupPositionPieceProperties />
            </div>
        </>
    );
};
export default SetupAnalysisPositionPage;
