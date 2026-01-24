import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { LogicalPoint } from "@/features/point/types";

export interface ClockSnapshot {
    whiteClock: number;
    blackClock: number;
}

export interface OvertimePendingRemovalNotification {
    encodedLegalMoves: string;
    removePieceAt: LogicalPoint;
}

export interface PlayerOvertime {
    secondRemainderMs: number;
    pendingRemoval: PendingOvertimeRemoval[];
}

export interface PendingOvertimeRemoval {
    legalMoves: LegalMoves;
    removedPieceAt: LogicalPoint;
}
