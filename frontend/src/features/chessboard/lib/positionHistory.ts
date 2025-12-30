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

export interface PositionProps {
    pieces: BoardPieces;
    move: Move;
    san: string;
}

export default class PositionHistory {
    [immerable] = true;

    _plyCount = 0;

    _rootPieces: BoardPieces;
    _byPositionId: Map<PositionId, PositionNode> = new Map();

    _head: PositionNode | null = null;
    _tail: PositionNode | null = null;

    _viewingPosition: PositionNode | null = null;

    constructor(rootPieces: BoardPieces) {
        this._rootPieces = rootPieces;
    }

    get rootPieces(): BoardPieces {
        return this._rootPieces;
    }

    get viewingPosition(): Position | null {
        return this._viewingPosition;
    }

    get plyCount(): number {
        return this._plyCount;
    }

    goToPosition(positionId: PositionId): Position | null {
        const node = this._byPositionId.get(positionId);
        if (!node) return null;

        this._viewingPosition = node;
        return node;
    }

    goToStart(): Position | null {
        this._viewingPosition = this._head;
        return this._head;
    }

    goToEnd(): Position | null {
        this._viewingPosition = this._tail;
        return this._tail;
    }

    stepBackward(): Position | null {
        if (!this._viewingPosition) return null;

        const prev = this._viewingPosition.prev;
        if (!prev) return null;

        this._viewingPosition = prev;
        return prev;
    }

    stepForward(): Position | null {
        if (!this._viewingPosition) return null;

        const next = this._viewingPosition.next;
        if (!next) return null;

        this._viewingPosition = next;
        return next;
    }

    addNextPosition(props: PositionProps): Position {
        // we're already viewing a position, add to it
        if (this._viewingPosition) {
            const node = this._addToNode(this._viewingPosition, props);
            this._viewingPosition = node;
            return node;
        }

        // we're empty, start the tree
        if (!this._head || !this._tail) {
            const node = new PositionNode(props);
            this._head = node;
            this._tail = node;
            this._plyCount = 1;

            this._byPositionId.set(node.positionId, node);
            this._viewingPosition = node;
            return node;
        }

        // we're not viewing anything, but we're not empty, expand tail
        const node = this._addToNode(this._tail, props);
        this._viewingPosition = node;
        return node;
    }

    private _addToNode(
        parent: PositionNode,
        position: PositionProps,
    ): PositionNode {
        const node = parent.createChild(position);

        // if parent is the tail, it cannot possibly have a sub variation already
        // which means this is a main variation, increment ply count
        if (parent === this._tail) {
            this._tail = node;
            this._plyCount++;
        }

        this._byPositionId.set(node.positionId, node);
        return node;
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

    _parent: PositionNode | null = null;
    _mainVariation: PositionNode | null = null;
    _subVariationBySan: Map<string, PositionNode> = new Map();

    constructor(props: PositionProps, parent: PositionNode | null = null) {
        this._parent = parent;
        this._pieces = props.pieces;
        this._move = props.move;
        this._san = props.san;
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

    get prev(): PositionNode | null {
        return this._parent;
    }
    get next(): PositionNode | null {
        return this._mainVariation;
    }

    get variations(): readonly PositionNode[] {
        const allVariations = [...this._subVariationBySan.values()];
        if (this._mainVariation) allVariations.unshift(this._mainVariation);

        return allVariations;
    }

    createChild(props: PositionProps): PositionNode {
        const child = new PositionNode(props, this);

        if (!this._mainVariation) {
            this._mainVariation = child;
            return child;
        }

        if (this._mainVariation.san === props.san) {
            return this._mainVariation;
        }

        const existingSubWithSan = this._subVariationBySan.get(props.san);
        if (existingSubWithSan) return existingSubWithSan;

        this._subVariationBySan.set(child.san, child);
        return child;
    }

    *[Symbol.iterator](): IterableIterator<PositionNode> {
        yield this;
        if (this._mainVariation) yield* this._mainVariation;
    }
}
