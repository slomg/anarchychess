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
        specialType: null,
        ...minimalMove,
    };
}
