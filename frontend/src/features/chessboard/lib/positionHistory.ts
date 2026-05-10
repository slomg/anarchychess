import { immerable } from "immer";

import {
    ChildPosition,
    PositionId,
    ChildPositionNode,
    PositionProps,
    RootPositionNode,
    Position,
    RootPositionProps,
} from "./position";

import { GameColor } from "@/lib/apiClient";
import { encodeFen } from "./fenEncoder";
import { MoveKey } from "./types";

export default class PositionHistory {
    [immerable] = true;

    _byPositionId: Map<PositionId, ChildPositionNode> = new Map();
    _byPly: Map<number, ChildPositionNode> = new Map();

    _root: RootPositionNode;
    _tail: ChildPositionNode | null = null;

    _currentNode: ChildPositionNode | null = null;

    constructor(props: RootPositionProps) {
        const pieces = props.pieces;
        const sideToMove = props.sideToMove ?? GameColor.WHITE;
        const fen = props.fen ?? encodeFen({ pieces, sideToMove });
        this._root = new RootPositionNode({ pieces, fen, sideToMove, ply: 0 });
    }

    get root(): Position {
        return this._root;
    }

    get tail(): ChildPosition | null {
        return this._tail;
    }

    get rootSubVariationByKey(): ReadonlyMap<MoveKey, ChildPosition> {
        return this._root.subVariationByKey;
    }

    get currentNode(): ChildPosition | null {
        return this._currentNode;
    }

    get currentPosition(): Position {
        return this._currentNode ?? this._root;
    }

    get mainPlyCount(): number {
        return this._tail?.ply ?? 0;
    }

    get totalPlyCount(): number {
        return this._byPositionId.size;
    }

    get isViewingLatestPosition(): boolean {
        return this._currentNode?.positionId === this._tail?.positionId;
    }

    overrideRoot(props: RootPositionProps) {
        const pieces = props.pieces;
        const sideToMove = props.sideToMove ?? this.currentPosition.sideToMove;
        console.log(props.sideToMove, sideToMove);
        const fen = props.fen ?? encodeFen({ pieces, sideToMove });
        this._root = new RootPositionNode({ pieces, fen, sideToMove, ply: 0 });

        this._tail = null;
        this._currentNode = null;
        this._byPositionId = new Map();
        this._byPly = new Map();
    }

    getPositionWithPly(ply: number): ChildPosition | undefined {
        return this._byPly.get(ply);
    }

    getNextPositionWithKey(key: MoveKey): ChildPosition | undefined {
        const currPosition = this._currentNode ? this._currentNode : this._root;

        if (currPosition.next?.move.moveKey === key) return currPosition.next;
        else return currPosition.subVariationByKey.get(key);
    }

    goToPosition(positionId: PositionId): {
        success: boolean;
        isOneStepForward: boolean;
    } {
        const node = this._byPositionId.get(positionId);
        if (!node) {
            return { success: false, isOneStepForward: false };
        }

        const isOneStepForward = this.currentPosition.isPositionNext(node);

        this._currentNode = node;
        return { success: true, isOneStepForward };
    }

    goToStart(): boolean {
        if (this._currentNode === null) return false;

        this._currentNode = null;
        return true;
    }

    goToEnd(): {
        success: boolean;
        isOneStepForward: boolean;
    } {
        if (this._currentNode?.positionId === this._tail?.positionId)
            return { success: false, isOneStepForward: false };

        const isOneStepForward =
            this._currentNode?.isPositionNext(this._tail) ?? false;
        this._currentNode = this._tail;
        return { success: true, isOneStepForward };
    }

    stepBackward(): boolean {
        if (!this._currentNode) return false;

        const prev = this._currentNode.prev;
        if (!prev) {
            this._currentNode = null;
            return true;
        }

        this._currentNode = prev;
        return true;
    }

    stepForward(): boolean {
        if (!this._currentNode && !this._root.next) return false;
        if (!this._currentNode) {
            this._currentNode = this._root.next;
            return true;
        }

        const next = this._currentNode.next;
        if (!next) return false;

        this._currentNode = next;
        return true;
    }

    addNextPosition(props: PositionProps): ChildPosition {
        const parent = this._currentNode ?? this._root;
        const { child: nextPosition, isMainVariation } =
            parent.createChild(props);
        this._trackPosition(nextPosition);

        if (isMainVariation) {
            this._tail = nextPosition;
            this._byPly.set(nextPosition.ply, nextPosition);
        }

        return nextPosition;
    }

    addNextSidelinePosition(props: PositionProps): ChildPosition {
        const parent = this._currentNode ?? this._root;
        let nextPosition: ChildPositionNode;

        // if viewing the tail, create a sub variation
        // if NOT viewing the tail, it's safe to call createChild because we're either viewing a position that is
        // - off the main line, so adding a main variation won't affect the main line
        // - on the main line but not the tail, so it must already have a main variation and calling createChild will not replace the main variation
        if (this._currentNode?.positionId === this._tail?.positionId) {
            nextPosition = parent.createSubVariationChild(props);
        } else {
            nextPosition = parent.createChild(props).child;
        }
        this._trackPosition(nextPosition);

        return nextPosition;
    }

    _trackPosition(position: ChildPositionNode) {
        this._byPositionId.set(position.positionId, position);
        this._currentNode = position;
    }

    *[Symbol.iterator](): IterableIterator<ChildPosition> {
        yield* this._root;
    }
}
