import { LogicalPoint, StrPoint } from "@/features/point/types";
import { pointToStr } from "@/features/point/pointUtils";
import { Move, Piece, PieceID } from "./types";

interface SinglePieceMove {
    pieceId: PieceID;
    from: LogicalPoint;
    to: LogicalPoint;
}

interface GatheredMoves {
    pieceMoves: SinglePieceMove[];
    movedPieceIds: Set<PieceID>;
}

export default class BoardPieces {
    _byId: Map<PieceID, Piece>;
    _byPosition: Map<StrPoint, PieceID>;
    _stunnedPieces: Map<PieceID, number>;

    constructor(other: BoardPieces | null = null) {
        if (other === null) {
            this._byId = new Map();
            this._byPosition = new Map();
            this._stunnedPieces = new Map();
            return;
        }

        this._byId = new Map(
            [...other._byId].map(([id, piece]) => {
                return [id, { ...piece }];
            }),
        );
        this._byPosition = new Map(other._byPosition);
        this._stunnedPieces = new Map(other._stunnedPieces);
    }

    static fromPieces(...pieces: Piece[]): BoardPieces {
        const boardPieces = new BoardPieces();
        for (const piece of pieces) {
            boardPieces.add(piece);
        }
        return boardPieces;
    }

    getById(pieceId: PieceID): Piece | undefined {
        return this._byId.get(pieceId);
    }

    getByPosition(position: LogicalPoint): Piece | undefined {
        const pieceId = this._byPosition.get(pointToStr(position));
        if (!pieceId) return undefined;

        return this._byId.get(pieceId);
    }

    playMove(move: Move): PieceID[] {
        const { pieceMoves, movedPieceIds } = this._gatherMoves(move);

        this._decrementStuns();
        for (const stun of move.stuns) {
            const piece = this.getByPosition(stun.position);
            if (!piece) {
                continue;
            }

            this._stunnedPieces.set(piece.id, stun.stunForTurns);
            piece.stunnedForTurns = stun.stunForTurns;
        }

        // step 1: remove all captures first
        // so we don't capture any piece that just moved
        this.removeRemovedPiecesFromMove(move);

        const capturedBeforeMove = new Set<PieceID>();
        // step 2: clear all origin squares of moving pieces
        // this is done before placing pieces to handle swaps correctly
        for (const pieceMove of pieceMoves) {
            if (this.getByPosition(pieceMove.from)) {
                this._byPosition.delete(pointToStr(pieceMove.from));
            } else {
                capturedBeforeMove.add(pieceMove.pieceId);
            }
        }

        // step 3: place all pieces on their final destinations
        for (const pieceMove of pieceMoves) {
            if (capturedBeforeMove.has(pieceMove.pieceId)) {
                continue;
            }

            this._byPosition.set(pointToStr(pieceMove.to), pieceMove.pieceId);
            const piece = this._byId.get(pieceMove.pieceId);
            if (piece) {
                piece.position = pieceMove.to;
                piece.hasMoved = true;
            }
        }

        for (const spawn of move.pieceSpawns) {
            this.add(spawn);
            movedPieceIds.add(spawn.id);
        }

        if (move.promotesTo !== null) {
            this.getByPosition(move.to)!.type = move.promotesTo;
        }

        return [...movedPieceIds];
    }

    _decrementStuns() {
        for (const [id, stunnedForTurns] of this._stunnedPieces) {
            const piece = this.getById(id);
            if (!piece) {
                this._stunnedPieces.delete(id);
                continue;
            }

            if (stunnedForTurns <= 1) {
                piece.stunnedForTurns = 0;
                this._stunnedPieces.delete(id);
            } else {
                piece.stunnedForTurns--;
                this._stunnedPieces.set(id, stunnedForTurns - 1);
            }
        }
    }

    removeRemovedPiecesFromMove(move: Move): Map<PieceID, Piece> {
        const removedPieces: Map<PieceID, Piece> = new Map();

        for (const capture of [...move.captures, ...move.overtimeRemovals]) {
            const capturedPiece = this.getByPosition(capture);
            if (capturedPiece) {
                this.remove(capturedPiece.id);
                removedPieces.set(capturedPiece.id, capturedPiece);
            }
        }

        return removedPieces;
    }

    movePiece(pieceId: PieceID, to: LogicalPoint) {
        const piece = this._byId.get(pieceId);
        if (!piece) return;

        this._byPosition.delete(pointToStr(piece.position));
        piece.position = to;

        const newPositionStr = pointToStr(to);
        const inNewPosition = this._byPosition.get(newPositionStr);
        if (inNewPosition) this._byId.delete(inNewPosition);
        this._byPosition.set(newPositionStr, pieceId);
    }

    add(piece: Piece): void {
        this.removeFrom(piece.position);

        this._byId.set(piece.id, { ...piece });
        this._byPosition.set(pointToStr(piece.position), piece.id);
        if (piece.stunnedForTurns > 0) {
            this._stunnedPieces.set(piece.id, piece.stunnedForTurns);
        }
    }

    addAt(piece: Piece, position: LogicalPoint): void {
        this.removeFrom(position);

        const newPiece = { ...piece, position };
        this._byId.set(newPiece.id, newPiece);
        this._byPosition.set(pointToStr(position), newPiece.id);
        if (piece.stunnedForTurns > 0) {
            this._stunnedPieces.set(piece.id, piece.stunnedForTurns);
        }
    }

    remove(pieceId: PieceID): boolean {
        const piece = this.getById(pieceId);
        if (!piece) {
            return false;
        }

        this._byId.delete(pieceId);
        this._byPosition.delete(pointToStr(piece.position));
        return true;
    }

    removeFrom(position: LogicalPoint): boolean {
        const piece = this.getByPosition(position);
        if (!piece) {
            return false;
        }

        this._byId.delete(piece.id);
        this._byPosition.delete(pointToStr(position));
        return true;
    }

    values(): IterableIterator<Piece> {
        return this._byId.values();
    }

    keys(): IterableIterator<PieceID> {
        return this._byId.keys();
    }

    get size(): number {
        return this._byId.size;
    }

    *[Symbol.iterator](): IterableIterator<Piece> {
        yield* this._byId.values();
    }

    _gatherMoves(move: Move): GatheredMoves {
        const pieceMoves: SinglePieceMove[] = [];
        const movedPieceIds = new Set<PieceID>();

        const mainPieceId = this._byPosition.get(pointToStr(move.from));
        if (mainPieceId) {
            pieceMoves.push({
                pieceId: mainPieceId,
                from: move.from,
                to: move.to,
            });
            movedPieceIds.add(mainPieceId);
        }

        for (const sideEffect of move.sideEffects) {
            const sideEffectPieceId = this._byPosition.get(
                pointToStr(sideEffect.from),
            );
            if (!sideEffectPieceId) {
                console.warn(
                    "Could not find side effect piece at",
                    pointToStr(sideEffect.from),
                );
                continue;
            }

            pieceMoves.push({
                pieceId: sideEffectPieceId,
                from: sideEffect.from,
                to: sideEffect.to,
            });
            movedPieceIds.add(sideEffectPieceId);
        }

        return { pieceMoves, movedPieceIds };
    }
}
