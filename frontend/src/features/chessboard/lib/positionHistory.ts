import { immerable } from "immer";

import BoardPieces from "./boardPieces";
import { Move } from "./types";

export type PositionId = string & { __brand: "PositionId" };

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
    _headVariationBySan: Map<string, PositionNode> = new Map();
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

    get isViewingLatestPosition(): boolean {
        return this._viewingPosition?.positionId === this._tail?.positionId;
    }

    goToPosition(positionId: PositionId): {
        success: boolean;
        isOneStepForward: boolean;
    } {
        const node = this._byPositionId.get(positionId);
        if (!node) return { success: false, isOneStepForward: false };

        let isOneStepForward = true;
        if (this._viewingPosition) {
            isOneStepForward =
                this._viewingPosition.next?.positionId === node.positionId ||
                this._viewingPosition.subVariationBySan.get(node.san)
                    ?.positionId === node.positionId;
        }

        this._viewingPosition = node;
        return { success: true, isOneStepForward };
    }

    goToStart(): void {
        this._viewingPosition = null;
    }

    goToEnd(): void {
        this._viewingPosition = this._tail;
    }

    stepBackward(): boolean {
        if (!this._viewingPosition) return false;

        const prev = this._viewingPosition.prev;
        if (!prev) {
            this._viewingPosition = null;
            return true;
        }

        this._viewingPosition = prev;
        return true;
    }

    stepForward(): boolean {
        if (!this._viewingPosition && !this._head) return false;
        if (!this._viewingPosition) {
            this._viewingPosition = this._head;
            return true;
        }

        const next = this._viewingPosition.next;
        if (!next) return false;

        this._viewingPosition = next;
        return true;
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

        // we're not viewing anything, but we're not empty, add a head variation
        const existing =
            props.san === this._head.san
                ? this._head
                : this._headVariationBySan.get(props.san);
        if (existing) {
            this._viewingPosition = existing;
            return existing;
        }

        const node = new PositionNode(props);
        this._byPositionId.set(node.positionId, node);
        this._headVariationBySan.set(props.san, node);
        this._viewingPosition = node;
        return node;
    }

    _addToNode(parent: PositionNode, position: PositionProps): PositionNode {
        const node = parent.createChild(position);

        // if parent is the tail, it cannot possibly have a sub variation already
        // which means this is a main variation, increment ply count
        if (parent?.positionId === this._tail?.positionId) {
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

    get subVariationBySan(): ReadonlyMap<string, PositionNode> {
        return this._subVariationBySan;
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
