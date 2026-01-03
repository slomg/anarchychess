import { immerable } from "immer";

import { Position, PositionId, PositionNode, PositionProps } from "./position";
import BoardPieces from "./boardPieces";
import { MoveKey } from "./types";

export default class PositionHistory {
    [immerable] = true;

    _mainBranchPlies = 0;

    _rootPieces: BoardPieces;
    _byPositionId: Map<PositionId, PositionNode> = new Map();

    _head: PositionNode | null = null;
    _headVariationByKey: Map<MoveKey, PositionNode> = new Map();
    _tail: PositionNode | null = null;

    _viewingPosition: PositionNode | null = null;

    constructor(rootPieces: BoardPieces) {
        this._rootPieces = rootPieces;
    }

    get rootPieces(): BoardPieces {
        return this._rootPieces;
    }

    get rootSubVariationByKey(): ReadonlyMap<MoveKey, Position> {
        return this._headVariationByKey;
    }

    get viewingPosition(): Position | null {
        return this._viewingPosition;
    }

    get mainPlyCount(): number {
        return this._mainBranchPlies;
    }

    get totalPlyCount(): number {
        return this._byPositionId.size;
    }

    get isViewingLatestPosition(): boolean {
        return this._viewingPosition?.positionId === this._tail?.positionId;
    }

    getNextPositionWithKey(key: MoveKey): Position | undefined {
        if (!this._viewingPosition) {
            const head = this._head?.move.moveKey === key && this._head;
            const headSub = this._headVariationByKey.get(key);
            return head || headSub;
        }

        const next =
            this._viewingPosition.next?.move.moveKey === key &&
            this._viewingPosition.next;
        const viewingSub = this._viewingPosition.subVariationByKey.get(key);
        return next || viewingSub;
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
                this._viewingPosition.subVariationByKey.get(node.move.moveKey)
                    ?.positionId === node.positionId;
        }

        this._viewingPosition = node;
        return { success: true, isOneStepForward };
    }

    goToStart(): boolean {
        if (this._viewingPosition === null) return false;

        this._viewingPosition = null;
        return true;
    }

    goToEnd(): {
        success: boolean;
        isOneStepForward: boolean;
    } {
        if (this._viewingPosition?.positionId === this._tail?.positionId)
            return { success: false, isOneStepForward: false };

        const isOneStepForward =
            this._viewingPosition?.next?.positionId === this._tail?.positionId;
        this._viewingPosition = this._tail;
        return { success: true, isOneStepForward };
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
            this._mainBranchPlies = 1;

            this._byPositionId.set(node.positionId, node);
            this._viewingPosition = node;
            return node;
        }

        // we're not viewing anything, but we're not empty, add a head variation
        const existing =
            props.move.moveKey === this._head.move.moveKey
                ? this._head
                : this._headVariationByKey.get(props.move.moveKey);
        if (existing) {
            this._viewingPosition = existing;
            return existing;
        }

        const node = new PositionNode(props);
        this._byPositionId.set(node.positionId, node);
        this._headVariationByKey.set(props.move.moveKey, node);
        this._viewingPosition = node;
        return node;
    }

    _addToNode(parent: PositionNode, position: PositionProps): PositionNode {
        const node = parent.createChild(position);

        // if parent is the tail, it cannot possibly have a sub variation already
        // which means this is a main variation, increment ply count
        if (parent?.positionId === this._tail?.positionId) {
            this._tail = node;
            this._mainBranchPlies++;
        }

        this._byPositionId.set(node.positionId, node);
        return node;
    }

    *[Symbol.iterator](): IterableIterator<Position> {
        if (this._head) yield* this._head;
    }
}
