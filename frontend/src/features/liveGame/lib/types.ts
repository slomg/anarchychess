import { LogicalPoint } from "@/features/point/types";

export interface ClockSnapshot {
    whiteClock: number;
    blackClock: number;
}

export interface OvertimePendingRemovalNotification {
    encodedLegalMoves: string;
    removeFrom: LogicalPoint;
}
