import { GameColor, PieceType } from "@/lib/apiClient";
import { logicalPoint, pointToStr } from "@/features/point/pointUtils";
import { LogicalPoint, StrPoint } from "@/features/point/types";
import { Move } from "@/features/chessboard/lib/types";
import { Piece } from "@/features/chessboard/lib/types";
import { faker } from "@faker-js/faker";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import LegalMoves from "@/features/chessboard/lib/legalMoves";

export function createRandomPoint(): LogicalPoint {
    return logicalPoint({
        x: faker.number.int({ min: 0, max: 99 }),
        y: faker.number.int({ min: 0, max: 99 }),
    });
}

export function createFakePiece(override?: Partial<Piece>): Piece {
    return {
        id: faker.string.uuid(),
        type: faker.helpers.enumValue(PieceType),
        color: faker.helpers.enumValue(GameColor),
        position: createRandomPoint(),
        ...override,
    };
}

export function createFakeMove(override?: Partial<Move>): Move {
    return {
        from: createRandomPoint(),
        to: createRandomPoint(),
        moveKey: faker.string.alpha(10),
        triggers: [],
        captures: [],
        intermediates: [],
        sideEffects: [],
        pieceSpawns: [],
        promotesTo: null,
        specialType: null,
        ...override,
    };
}

export function createFakeBoardPieces(count = 5): BoardPieces {
    const boardPieces = new BoardPieces();
    for (let i = 0; i < count; i++) {
        boardPieces.add(createFakePiece({ id: i.toString() }));
    }
    return boardPieces;
}

export function createFakeLegalMoves({
    count,
    hasForcedMoves,
}: {
    count?: number;
    hasForcedMoves?: boolean;
} = {}): LegalMoves {
    count ??= 5;

    const pieces: Piece[] = [];
    for (let i = 0; i < count; i++) pieces.push(createFakePiece());

    return createFakeLegalMovesFromPieces({ pieces, hasForcedMoves });
}

export function createFakeLegalMovesFromPieces({
    pieces,
    hasForcedMoves,
}: { pieces?: Piece[]; hasForcedMoves?: boolean } = {}): LegalMoves {
    pieces ??= [];

    const map = new Map<StrPoint, Move[]>();
    for (const piece of pieces) {
        map.set(pointToStr(piece.position), [
            createFakeMove({ from: piece.position }),
            createFakeMove({ from: piece.position }),
        ]);
    }
    return new LegalMoves(map, hasForcedMoves);
}

export function createFakeLegalMovesFromMoves({
    moves,
    hasForcedMoves,
}: { moves?: Move[]; hasForcedMoves?: boolean } = {}): LegalMoves {
    moves ??= [];

    const map = new Map<StrPoint, Move[]>();
    for (const move of moves) {
        map.set(pointToStr(move.from), [move]);
    }
    return new LegalMoves(map, hasForcedMoves);
}
