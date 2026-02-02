import { useState } from "react";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import ChessSquare from "@/features/chessboard/components/ChessSquare";
import useLiveChessStore from "../hooks/useLiveChessStore";
import { LogicalPoint } from "@/features/point/types";
import { useGameEvent } from "../hooks/useGameHub";

const OvertimeAlert = () => {
    const { gameToken, resultData } = useLiveChessStore((x) => ({
        gameToken: x.gameToken,
        resultData: x.resultData,
    }));
    const plyNumber = useChessboardStore((x) => x.positionHistory.mainPlyCount);

    const [alert, setAlert] = useState<{
        plyNumber: number;
        removeFrom: LogicalPoint;
    } | null>(null);

    useGameEvent(
        gameToken,
        "ReceiveNextOvertimeAsync",
        (plyNumber, removeFrom) => {
            setAlert({ plyNumber, removeFrom });
        },
    );

    if (
        alert === null ||
        resultData !== null ||
        alert.plyNumber !== plyNumber
    ) {
        return null;
    }

    return <OvertimeSquare position={alert.removeFrom} />;
};
export default OvertimeAlert;

const OvertimeSquare = ({ position }: { position: LogicalPoint }) => {
    return (
        <ChessSquare
            data-testid="overtimeSquare"
            className="animate-fade-in before:animate-freakout z-20
                before:absolute before:inset-0 before:border-4
                before:border-red-500 sm:before:border-6"
            position={position}
        />
    );
};
