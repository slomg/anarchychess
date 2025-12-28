import { immerable } from "immer";

import { LogicalPoint, StrPoint } from "@/features/point/types";
import { Move } from "./types";
import { pointEquals, pointToStr } from "@/features/point/pointUtils";

export default class LegalMoves {
    [immerable] = true;

    _legalMoves: Map<StrPoint, Move[]>;
    _hasForcedMoves: boolean;

    constructor(
        legalMoves: Map<StrPoint, Move[]> | [StrPoint, Move[]][] | null = null,
        hasForcedMoves = false,
    ) {
        if (legalMoves instanceof Map) {
            this._legalMoves = legalMoves;
        } else if (Array.isArray(legalMoves)) {
            this._legalMoves = new Map(legalMoves);
        } else {
            this._legalMoves = new Map();
        }

        this._hasForcedMoves = hasForcedMoves;
    }

    get hasForcedMoves(): boolean {
        return this._hasForcedMoves;
    }

    hasMovesFromTo(from: LogicalPoint, to: LogicalPoint): boolean {
        const movesFromOrigin = this._legalMoves.get(pointToStr(from));
        if (!movesFromOrigin) return false;

        return movesFromOrigin.some((move) => pointEquals(move.to, to));
    }

    get size(): number {
        return this._legalMoves.size;
    }

    get(position: LogicalPoint): Move[] | null {
        const moves = this._legalMoves.get(pointToStr(position));
        return moves ?? null;
    }

    *[Symbol.iterator](): IterableIterator<Move[]> {
        yield* this._legalMoves.values();
    }
}
