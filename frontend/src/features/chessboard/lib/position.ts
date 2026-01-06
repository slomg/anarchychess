import { GameColor } from "@/lib/apiClient";
import BoardPieces from "./boardPieces";
import { Move, MoveKey } from "./types";

export type PositionId = string & { __brand: "PositionId" };

export interface PositionProps {
    pieces: BoardPieces;
    fen: string;
    sideToMove: GameColor;
    move: Move;
    san: string;
}

export interface Position {
    pieces: BoardPieces;
    fen: string;
    sideToMove: GameColor;
    move: Move;
    san: string;
    ply: number;

    positionId: PositionId;
    variations: readonly Position[];
    subVariationByKey: ReadonlyMap<MoveKey, Position>;
    [Symbol.iterator](): IterableIterator<Position>;
}

export abstract class PositionNode {
    _pieces: BoardPieces;

    _mainVariation: ChildPositionNode | null = null;
    _subVariationByKey: Map<MoveKey, ChildPositionNode> = new Map();
    _allVariations: ChildPositionNode[] = [];

    _positionId: PositionId = crypto.randomUUID() as PositionId;

    constructor(pieces: BoardPieces) {
        this._pieces = pieces;
    }

    get pieces(): BoardPieces {
        return this._pieces;
    }

    get positionId(): PositionId {
        return this._positionId;
    }

    get next(): ChildPositionNode | null {
        return this._mainVariation;
    }

    get variations(): readonly Position[] {
        return this._allVariations;
    }

    get subVariationByKey(): ReadonlyMap<MoveKey, Position> {
        return this._subVariationByKey;
    }

    isPositionNext(position: ChildPositionNode | null): boolean {
        if (position === null) return false;

        return (
            position.positionId === this.next?.positionId ||
            this.subVariationByKey.get(position.move.moveKey)?.positionId ===
                position.positionId
        );
    }

    createChild(props: PositionProps): {
        child: ChildPositionNode;
        isMainVariation: boolean;
    } {
        const child = new ChildPositionNode(
            props,
            this instanceof ChildPositionNode ? this : null,
        );

        if (!this._mainVariation) {
            this._mainVariation = child;
            this._allVariations.push(child);
            return { child, isMainVariation: true };
        }

        if (this._mainVariation?.move.moveKey === props.move.moveKey) {
            return { child: this._mainVariation, isMainVariation: true };
        }

        const existingSubWithSan = this._subVariationByKey.get(
            props.move.moveKey,
        );
        if (existingSubWithSan)
            return { child: existingSubWithSan, isMainVariation: false };

        this._subVariationByKey.set(child.move.moveKey, child);
        this._allVariations.push(child);
        return { child, isMainVariation: false };
    }

    *[Symbol.iterator](): IterableIterator<ChildPositionNode> {
        if (this._mainVariation) yield* this._mainVariation;
    }
}

export class RootPositionNode extends PositionNode {}

export class ChildPositionNode extends PositionNode implements Position {
    _fen: string;
    _sideToMove: GameColor;
    _move: Move;
    _san: string;
    _ply: number;

    _parent: ChildPositionNode | null = null;

    constructor(props: PositionProps, parent: ChildPositionNode | null = null) {
        super(props.pieces);
        this._parent = parent;

        this._pieces = props.pieces;
        this._fen = props.fen;
        this._sideToMove = props.sideToMove;
        this._move = props.move;
        this._san = props.san;
        this._ply = parent ? parent.ply + 1 : 0;
    }

    get fen(): string {
        return this._fen;
    }

    get sideToMove(): GameColor {
        return this._sideToMove;
    }

    get move(): Move {
        return this._move;
    }

    get san(): string {
        return this._san;
    }

    get ply(): number {
        return this._ply;
    }

    get prev(): ChildPositionNode | null {
        return this._parent;
    }

    override *[Symbol.iterator](): IterableIterator<ChildPositionNode> {
        yield this;
        if (this._mainVariation) yield* this._mainVariation;
    }
}
