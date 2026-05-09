import { immerable } from "immer";

import {
    Position,
    PositionId,
    ChildPositionNode,
    PositionProps,
    RootPositionNode,
    RootPosition,
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

    _viewingPosition: ChildPositionNode | null = null;

    constructor(props: RootPositionProps) {
        const pieces = props.pieces;
        const sideToMove = props.sideToMove ?? GameColor.WHITE;
        const fen = props.fen ?? encodeFen({ pieces, sideToMove });
        this._root = new RootPositionNode(pieces, fen, sideToMove);
    }

    get root(): RootPosition {
        return this._root;
    }

    get tail(): Position | null {
        return this._tail;
    }

    get rootSubVariationByKey(): ReadonlyMap<MoveKey, Position> {
        return this._root.subVariationByKey;
    }

    get viewingPosition(): Position | null {
        return this._viewingPosition;
    }

    get mainPlyCount(): number {
        return this._tail?.ply ?? 0;
    }

    get totalPlyCount(): number {
        return this._byPositionId.size;
    }

    get isViewingLatestPosition(): boolean {
        return this._viewingPosition?.positionId === this._tail?.positionId;
    }

    overrideRoot(props: RootPositionProps) {
        const pieces = props.pieces;
        const sideToMove = props.sideToMove ?? GameColor.WHITE;
        const fen = props.fen ?? encodeFen({ pieces, sideToMove });
        this._root = new RootPositionNode(pieces, fen, sideToMove);

        this._tail = null;
        this._viewingPosition = null;
        this._byPositionId = new Map();
        this._byPly = new Map();
    }

    getPositionWithPly(ply: number): Position | undefined {
        return this._byPly.get(ply);
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
        if (!node) {
            return { success: false, isOneStepForward: false };
        }

        const viewingPosition = this._viewingPosition ?? this._root;
        const isOneStepForward = viewingPosition.isPositionNext(node);

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
        const parent = this._viewingPosition ?? this._root;
        const { child: nextPosition, isMainVariation } =
            parent.createChild(props);
        this._trackPosition(nextPosition);

        if (isMainVariation) {
            this._tail = nextPosition;
            this._byPly.set(nextPosition.ply, nextPosition);
        }

        return nextPosition;
    }

    addNextSidelinePosition(props: PositionProps): Position {
        const parent = this._viewingPosition ?? this._root;
        let nextPosition: ChildPositionNode;

        // if viewing the tail, create a sub variation
        // if NOT viewing the tail, it's safe to call createChild because we're either viewing a position that is
        // - off the main line, so adding a main variation won't affect the main line
        // - on the main line but not the tail, so it must already have a main variation and calling createChild will not replace the main variation
        if (this._viewingPosition?.positionId === this._tail?.positionId) {
            nextPosition = parent.createSubVariationChild(props);
        } else {
            nextPosition = parent.createChild(props).child;
        }
        this._trackPosition(nextPosition);

        return nextPosition;
    }

    _trackPosition(position: ChildPositionNode) {
        this._byPositionId.set(position.positionId, position);
        this._viewingPosition = position;
    }

    *[Symbol.iterator](): IterableIterator<Position> {
        yield* this._root;
    }
}
