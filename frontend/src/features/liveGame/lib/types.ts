import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { LogicalPoint } from "@/features/point/types";

export interface ClockSnapshot {
    whiteClock: number;
    blackClock: number;
}

export interface PlayerOvertime {
    secondRemainder: number;
    pendingRemoval: PendingOvertimeRemoval[];
}

export interface PendingOvertimeRemoval {
    legalMoves: LegalMoves;
    removedPieceAt: LogicalPoint;
}
