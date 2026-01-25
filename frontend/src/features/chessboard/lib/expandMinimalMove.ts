import { ForcedMovePriority, SpecialMoveType } from "@/lib/apiClient";
import { MinimalMove, Move, MoveKey } from "./types";

export default function expandMinimalMove(minimalMove: MinimalMove): Move {
    return {
        moveKey: "" as MoveKey,
        triggers: [],
        captures: [],
        intermediates: [],
        sideEffects: [],
        pieceSpawns: [],
        promotesTo: null,
        specialType: SpecialMoveType.NONE,
        forcedPriority: ForcedMovePriority.NONE,
        emphasizeSquare: false,
        overtimeRemovals: [],
        ...minimalMove,
    };
}
