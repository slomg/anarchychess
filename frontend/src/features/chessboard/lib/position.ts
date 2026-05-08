import { GameColor } from "@/lib/apiClient";
import BoardPieces from "./boardPieces";
import { Move, MoveKey } from "./types";
import { LogicalPoint } from "@/features/point/types";

export type PositionId = string & { __brand: "PositionId" };

export interface PositionProps {
    pieces: BoardPieces;
    fen: string;
    sideToMove: GameColor;
    move: Move;
    san: string;
}

export interface RootPosition {
    positionId: PositionId;
    pieces: BoardPieces;
    fen: string;
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

    commitOvertimeRemoval(removeFrom: LogicalPoint): void;
    [Symbol.iterator](): IterableIterator<Position>;
}

export abstract class PositionNode {
    _pieces: BoardPieces;
    _fen: string;

    _mainVariation: ChildPositionNode | null = null;
    _subVariationByKey: Map<MoveKey, ChildPositionNode> = new Map();
    _allVariations: ChildPositionNode[] = [];

    _positionId: PositionId = crypto.randomUUID() as PositionId;

    constructor(pieces: BoardPieces, fen: string) {
        this._pieces = pieces;
        this._fen = fen;
    }

    get pieces(): BoardPieces {
        return this._pieces;
    }

    get fen(): string {
        return this._fen;
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
        if (!this._mainVariation) {
            const child = new ChildPositionNode(
                props,
                this instanceof ChildPositionNode ? this : null,
            );
            this._mainVariation = child;
            this._allVariations.push(child);
            return { child, isMainVariation: true };
        }

        if (this._mainVariation?.move.moveKey === props.move.moveKey) {
            return { child: this._mainVariation, isMainVariation: true };
        }

        const child = this.createSubVariationChild(props);
        return { child, isMainVariation: false };
    }

    createSubVariationChild(props: PositionProps): ChildPositionNode {
        if (this._mainVariation?.move.moveKey === props.move.moveKey) {
            return this._mainVariation;
        }

        const existingSubWithSan = this._subVariationByKey.get(
            props.move.moveKey,
        );
        if (existingSubWithSan) {
            return existingSubWithSan;
        }

        const child = new ChildPositionNode(
            props,
            this instanceof ChildPositionNode ? this : null,
        );
        this._subVariationByKey.set(child.move.moveKey, child);
        this._allVariations.push(child);
        return child;
    }

    *[Symbol.iterator](): IterableIterator<ChildPositionNode> {
        if (this._mainVariation) yield* this._mainVariation;
    }
}

export class RootPositionNode extends PositionNode implements RootPosition {}

export class ChildPositionNode extends PositionNode implements Position {
    _sideToMove: GameColor;
    _move: Move;
    _san: string;
    _ply: number;

    _parent: ChildPositionNode | null = null;

    constructor(props: PositionProps, parent: ChildPositionNode | null = null) {
        super(props.pieces, props.fen);
        this._parent = parent;

        this._pieces = new BoardPieces(props.pieces);
        this._fen = props.fen;
        this._sideToMove = props.sideToMove;
        this._move = props.move;
        this._san = props.san;
        this._ply = parent ? parent.ply + 1 : 1;
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

    commitOvertimeRemoval(removeFrom: LogicalPoint): void {
        this._pieces.removeFrom(removeFrom);
        this._move.overtimeRemovals.push(removeFrom);
    }

    override *[Symbol.iterator](): IterableIterator<ChildPositionNode> {
        yield this;
        if (this._mainVariation) yield* this._mainVariation;
    }
}
