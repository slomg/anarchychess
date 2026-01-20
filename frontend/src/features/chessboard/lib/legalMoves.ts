import { LogicalPoint, StrPoint } from "@/features/point/types";
import { pointToStr } from "@/features/point/pointUtils";
import { ForcedMovePriority } from "@/lib/apiClient";
import { Move } from "./types";

export interface MoveNode {
    from: LogicalPoint;
    at: LogicalPoint;
    terminalMoves: Move[];
    nextIntermediates: Map<StrPoint, MoveNode>;
}

export default class LegalMoves {
    readonly byOrigin: Map<StrPoint, Map<StrPoint, MoveNode>> = new Map();
    _hasForcedMoves: boolean = false;
    _emphasizedSquares: LogicalPoint[] = [];

    constructor(moves?: Move[]) {
        if (!moves) return;

        for (const move of moves) {
            this.addMove(move);
        }
    }

    get hasForcedMoves(): boolean {
        return this._hasForcedMoves;
    }

    get emphasizedSquares(): LogicalPoint[] {
        return this._emphasizedSquares;
    }

    hasMovesDirectlyFromTo(from: LogicalPoint, to: LogicalPoint): boolean {
        return this.getDirectNode(from, to) !== null;
    }

    getDirectNode(from: LogicalPoint, to: LogicalPoint): MoveNode | null {
        const fromMap = this.byOrigin.get(pointToStr(from));
        return fromMap?.get(pointToStr(to)) ?? null;
    }

    getFromOrigin(from: LogicalPoint): IterableIterator<MoveNode> {
        return (
            this.byOrigin.get(pointToStr(from))?.values() ??
            [][Symbol.iterator]()
        );
    }

    addMove(move: Move): void {
        if (move.forcedPriority != ForcedMovePriority.NONE) {
            this._hasForcedMoves = true;
        }
        if (move.emphasizeSquare) {
            this._emphasizedSquares.push(move.from);
        }

        let movesFromOrigin = this.byOrigin.get(pointToStr(move.from));
        if (!movesFromOrigin) {
            movesFromOrigin = new Map();
            this.byOrigin.set(pointToStr(move.from), movesFromOrigin);
        }

        for (const trigger of move.triggers) {
            this._insertMoveTree(move, movesFromOrigin, trigger);
        }
        this._insertMoveTree(move, movesFromOrigin, move.to);
    }

    _insertMoveTree(
        move: Move,
        movesFromOrigin: Map<StrPoint, MoveNode>,
        destination: LogicalPoint,
    ): void {
        // no intermediates, just add to root terminal moves
        if (move.intermediates.length === 0) {
            const current = this._getOrCreateNode(
                movesFromOrigin,
                destination,
                move.from,
            );
            current.terminalMoves.push(move);
            return;
        }

        // get the intermediate node root
        let current = this._getOrCreateNode(
            movesFromOrigin,
            move.intermediates[0].position,
            move.from,
        );
        // continue building the intermediate tree
        for (let i = 1; i < move.intermediates.length; i++) {
            current = this._getOrCreateNode(
                current.nextIntermediates,
                move.intermediates[i].position,
                move.from,
            );
        }
        // finally, add the terminal move at the destination
        current = this._getOrCreateNode(
            current.nextIntermediates,
            destination,
            move.from,
        );
        current.terminalMoves.push(move);
    }

    _getOrCreateNode(
        map: Map<StrPoint, MoveNode>,
        at: LogicalPoint,
        from: LogicalPoint,
    ): MoveNode {
        const atStr = pointToStr(at);
        let node = map.get(atStr);
        if (!node) {
            node = {
                from,
                at,
                terminalMoves: [],
                nextIntermediates: new Map(),
            };
            map.set(atStr, node);
        }
        return node;
    }
}
