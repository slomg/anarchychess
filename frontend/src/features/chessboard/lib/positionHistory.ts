import { immerable } from "immer";

import {
    PositionNode,
    Position,
    PositionId,
    ChildPositionNode,
    PositionProps,
    RootPositionNode,
} from "./position";

import BoardPieces from "./boardPieces";
import { MoveKey } from "./types";

export default class PositionHistory {
    [immerable] = true;

    _mainBranchPlies = 0;
    _byPositionId: Map<PositionId, ChildPositionNode> = new Map();

    _root: RootPositionNode;
    _tail: ChildPositionNode | null = null;

    _viewingPosition: ChildPositionNode | null = null;

    constructor(rootPieces: BoardPieces) {
        this._root = new RootPositionNode(rootPieces);
    }

    get rootPieces(): BoardPieces {
        return this._root.pieces;
    }

    get rootSubVariationByKey(): ReadonlyMap<MoveKey, Position> {
        return this._root.subVariationByKey;
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
        const currPosition = this._viewingPosition
            ? this._viewingPosition
            : this._root;

        if (currPosition.next?.move.moveKey === key) return currPosition.next;
        else return currPosition.subVariationByKey.get(key);
    }

    goToPosition(positionId: PositionId): {
        success: boolean;
        isOneStepForward: boolean;
    } {
        const node = this._byPositionId.get(positionId);
        if (!node) return { success: false, isOneStepForward: false };

        let isOneStepForward = true;
        if (this._viewingPosition) {
            isOneStepForward = this._viewingPosition.isPositionNext(node);
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
            this._viewingPosition?.isPositionNext(this._tail) ?? false;
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
        if (!this._viewingPosition && !this._root.next) return false;
        if (!this._viewingPosition) {
            this._viewingPosition = this._root.next;
            return true;
        }

        const next = this._viewingPosition.next;
        if (!next) return false;

        this._viewingPosition = next;
        return true;
    }

    addNextPosition(props: PositionProps): Position {
        return this._addToNode(props, this._viewingPosition ?? this._root);
    }

    _addToNode(props: PositionProps, parent: PositionNode): ChildPositionNode {
        const { child, isMainVariation } =
            parent?.createChild(props) ?? new ChildPositionNode(props);

        if (isMainVariation) {
            this._tail = child;
            this._mainBranchPlies++;
        }

        this._byPositionId.set(child.positionId, child);
        this._viewingPosition = child;
        return child;
    }

    *[Symbol.iterator](): IterableIterator<Position> {
        yield* this._root;
    }
}
