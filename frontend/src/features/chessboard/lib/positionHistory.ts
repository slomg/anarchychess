import { immerable } from "immer";

import { Move, PositionId } from "./types";
import BoardPieces from "./boardPieces";

export interface Position {
    pieces: BoardPieces;
    move: Move;
    san: string;

    variations: readonly Position[];
    positionId: PositionId;
}

export default class PositionHistory {
    [immerable] = true;

    _plyCount = 0;

    _rootPieces: BoardPieces;
    _byPositionId: Map<PositionId, PositionNode> = new Map();

    _head: PositionNode | null = null;
    _tail: PositionNode | null = null;

    constructor(rootPieces: BoardPieces) {
        this._rootPieces = rootPieces;
    }

    get rootPieces(): BoardPieces {
        return this._rootPieces;
    }

    get plyCount(): number {
        return this._plyCount;
    }

    getByPositionId(positionId: PositionId): Position | undefined {
        return this._byPositionId.get(positionId);
    }

    createMainPosition(pieces: BoardPieces, move: Move, san: string): Position {
        if (!this._head || !this._tail) {
            const node = new PositionNode(pieces, move, san);
            this._head = node;
            this._tail = node;
            this._plyCount = 1;

            this._byPositionId.set(node.positionId, node);
            return node;
        }

        const node = this._tail.createChild(pieces, move, san);
        this._setTail(node);

        return node;
    }

    addVariationToPosition(
        parent: Position,
        pieces: BoardPieces,
        move: Move,
        san: string,
    ): Position | undefined {
        const parentNode = this._byPositionId.get(parent.positionId);
        if (!parentNode) {
            console.warn(
                `Could not find parent node ${parent.positionId} in position history`,
            );
            return;
        }

        const node = parentNode.createChild(pieces, move, san);
        if (parentNode === this._tail) {
            this._setTail(node);
            return node;
        }

        this._byPositionId.set(node.positionId, node);
        return node;
    }

    private _setTail(node: PositionNode) {
        this._tail = node;
        this._byPositionId.set(node.positionId, node);
        this._plyCount++;
    }

    *[Symbol.iterator](): IterableIterator<Position> {
        if (this._head) yield* this._head;
    }
}

class PositionNode implements Position {
    [immerable] = true;

    _pieces: BoardPieces;
    _move: Move;
    _san: string;

    _positionId: PositionId = crypto.randomUUID() as PositionId;

    _mainVariation: PositionNode | null = null;
    _subVariationBySan: Map<string, PositionNode> = new Map();

    constructor(pieces: BoardPieces, move: Move, san: string) {
        this._pieces = pieces;
        this._move = move;
        this._san = san;
    }

    get move(): Move {
        return this._move;
    }

    get san(): string {
        return this._san;
    }

    get pieces(): BoardPieces {
        return this._pieces;
    }

    get positionId(): PositionId {
        return this._positionId;
    }

    get variations(): readonly PositionNode[] {
        const allVariations = [...this._subVariationBySan.values()];
        if (this._mainVariation) allVariations.unshift(this._mainVariation);

        return allVariations;
    }

    createChild(pieces: BoardPieces, move: Move, san: string): PositionNode {
        const child = new PositionNode(pieces, move, san);

        if (!this._mainVariation) {
            this._mainVariation = child;
            return child;
        }

        if (this._mainVariation.san === san) {
            return this._mainVariation;
        }

        const existingSubWithSan = this._subVariationBySan.get(san);
        if (existingSubWithSan) return existingSubWithSan;

        this._subVariationBySan.set(child.san, child);
        return child;
    }

    *[Symbol.iterator](): IterableIterator<PositionNode> {
        yield this;
        if (this._mainVariation) yield* this._mainVariation;
    }
}
