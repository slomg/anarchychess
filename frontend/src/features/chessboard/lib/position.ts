import { immerable } from "immer";

import type PositionHistory from "./positionHistory";
import { Move, PositionId } from "./types";
import BoardPieces from "./boardPieces";

export abstract class BasePosition {
    [immerable] = true;

    _pieces: BoardPieces;
    _positionId: PositionId = crypto.randomUUID() as PositionId;

    _mainBranch: Position | null = null;
    _subBranchBySan: Map<string, Position> = new Map();

    constructor(pieces: BoardPieces) {
        this._pieces = pieces;
    }

    get pieces(): BoardPieces {
        return this._pieces;
    }

    get positionId(): PositionId {
        return this._positionId;
    }

    get next(): Position | null {
        return this._mainBranch;
    }

    get subBranches(): readonly Position[] {
        return [...this._subBranchBySan.values()];
    }

    setNext(position: Position, positionHistory: PositionHistory): void {
        if (!this._mainBranch) {
            this._mainBranch = position;
        } else if (this._mainBranch.san !== position.san) {
            this._subBranchBySan.set(position.san, position);
        }

        positionHistory.registerPosition(position);
    }

    *[Symbol.iterator](): IterableIterator<Position> {
        if (this instanceof Position) yield this;
        if (this._mainBranch) yield* this._mainBranch;
    }
}

export class RootPosition extends BasePosition {}

export class Position extends BasePosition {
    _move: Move;
    _san: string;

    constructor(pieces: BoardPieces, move: Move, san: string) {
        super(pieces);

        this._move = move;
        this._san = san;
    }

    get move(): Move {
        return this._move;
    }

    get san(): string {
        return this._san;
    }
}
