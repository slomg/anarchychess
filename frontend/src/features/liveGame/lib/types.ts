import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { LogicalPoint } from "@/features/point/types";

export interface ClockSnapshot {
    whiteClock: number;
    blackClock: number;
}

export interface OvertimePendingRemovalNotification {
    encodedLegalMoves: string;
    removeFrom: LogicalPoint;
    removeAtTimestamp: number;
}

export interface PendingOvertimeRemoval {
    legalMoves: LegalMoves;
    removeFrom: LogicalPoint;
    removeAtTimestamp: number;
}
