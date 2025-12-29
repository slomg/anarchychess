import { immerable } from "immer";

import { BasePosition, Position, RootPosition } from "./position";
import { PositionId } from "./types";

export default class PositionHistory {
    [immerable] = true;

    _root: RootPosition;
    _byPositionId: Map<PositionId, BasePosition> = new Map();

    constructor(root: RootPosition) {
        this._root = root;
        this._byPositionId.set(root.positionId, root);
    }

    get root(): RootPosition {
        return this._root;
    }

    getByPositionId(positionId: PositionId): BasePosition | undefined {
        return this._byPositionId.get(positionId);
    }

    registerPosition(position: Position) {
        this._byPositionId.set(position.positionId, position);
    }
}
