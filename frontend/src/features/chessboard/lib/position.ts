import { GameColor } from "@/lib/apiClient";
import BoardPieces from "./boardPieces";
import { Move } from "./types";

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
    subVariationBySan: ReadonlyMap<string, Position>;
    [Symbol.iterator](): IterableIterator<Position>;
}

export class PositionNode implements Position {
    _pieces: BoardPieces;
    _fen: string;
    _sideToMove: GameColor;
    _move: Move;
    _san: string;
    _ply: number;

    _positionId: PositionId = crypto.randomUUID() as PositionId;

    _parent: PositionNode | null = null;
    _mainVariation: PositionNode | null = null;
    _subVariationBySan: Map<string, PositionNode> = new Map();
    _allVariations: PositionNode[] = [];

    constructor(props: PositionProps, parent: PositionNode | null = null) {
        this._parent = parent;

        this._pieces = props.pieces;
        this._fen = props.fen;
        this._sideToMove = props.sideToMove;
        this._move = props.move;
        this._san = props.san;
        this._ply = parent ? parent.ply + 1 : 0;
    }

    get pieces(): BoardPieces {
        return this._pieces;
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

    get positionId(): PositionId {
        return this._positionId;
    }

    get prev(): PositionNode | null {
        return this._parent;
    }

    get next(): PositionNode | null {
        return this._mainVariation;
    }

    get variations(): readonly Position[] {
        return this._allVariations;
    }

    get subVariationBySan(): ReadonlyMap<string, Position> {
        return this._subVariationBySan;
    }

    createChild(props: PositionProps): PositionNode {
        const child = new PositionNode(props, this);

        if (!this._mainVariation) {
            this._mainVariation = child;
            this._allVariations.push(child);
            return child;
        }

        if (this._mainVariation.san === props.san) {
            return this._mainVariation;
        }

        const existingSubWithSan = this._subVariationBySan.get(props.san);
        if (existingSubWithSan) return existingSubWithSan;

        this._subVariationBySan.set(child.san, child);
        this._allVariations.push(child);
        return child;
    }

    *[Symbol.iterator](): IterableIterator<PositionNode> {
        yield this;
        if (this._mainVariation) yield* this._mainVariation;
    }
}
export type PositionId = string & { __brand: "PositionId" };
