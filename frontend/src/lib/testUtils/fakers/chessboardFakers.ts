import { faker } from "@faker-js/faker";

import {
    ForcedMovePriority,
    GameColor,
    PieceType,
    SpecialMoveType,
} from "@/lib/apiClient";

import { Move, MoveKey } from "@/features/chessboard/lib/types";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { logicalPoint } from "@/features/point/pointUtils";
import { Piece } from "@/features/chessboard/lib/types";
import { LogicalPoint } from "@/features/point/types";

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
        moveKey: faker.string.uuid() as MoveKey,
        triggers: [],
        captures: [],
        intermediates: [],
        sideEffects: [],
        pieceSpawns: [],
        promotesTo: null,
        specialType: SpecialMoveType.NONE,
        forcedPriority: ForcedMovePriority.NONE,
        emphasizeSquare: false,
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

    const legalMoves = new LegalMoves();

    const forcedPriority = hasForcedMoves
        ? ForcedMovePriority.EN_PASSANT
        : ForcedMovePriority.NONE;
    for (const piece of pieces) {
        legalMoves.addMove(
            createFakeMove({
                from: piece.position,
                forcedPriority,
            }),
        );
        legalMoves.addMove(
            createFakeMove({
                from: piece.position,
                forcedPriority,
            }),
        );
    }

    return legalMoves;
}
