import { useState } from "react";

import { EmphasizedSquare } from "@/features/chessboard/components/EmphasizedSquare";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
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

    return <EmphasizedSquare position={alert.removeFrom} />;
};
export default OvertimeAlert;
