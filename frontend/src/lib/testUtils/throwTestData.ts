import { logicalPoint, offset } from "@/features/point/pointUtils";
import { createFakeMove, createFakePiece } from "./fakers/chessboardFakers";
import { Move, Piece } from "@/features/chessboard/lib/types";
import { LogicalPoint, Offset } from "@/features/point/types";
import { GameColor } from "../apiClient";

interface ThrowTestMovesData {
    sides: Move[][];
    all: Move[];
}

interface ThrowTestPointsData {
    sides: LogicalPoint[][];
    all: LogicalPoint[];
}

export interface ThrowTestData {
    direction: Offset;
    throwerOrigin: LogicalPoint;
    piece: Piece;
    moves: ThrowTestMovesData;
    points: ThrowTestPointsData;
}

const WHITE_PIECE = createFakePiece({
    position: logicalPoint({ x: 5, y: 1 }),
    color: GameColor.WHITE,
});
export const WHITE_LEFT_THROWS = createThrowTestData({
    direction: offset({ x: -1, y: 1 }),
    piece: WHITE_PIECE,
    throwerOrigin: logicalPoint({ x: 6, y: 0 }),
    leftPoints: [
        logicalPoint({ x: 3, y: 2 }),
        logicalPoint({ x: 2, y: 3 }),
        logicalPoint({ x: 1, y: 4 }),
        logicalPoint({ x: 0, y: 5 }),
    ],
    centerPoints: [
        logicalPoint({ x: 4, y: 2 }),
        logicalPoint({ x: 3, y: 3 }),
        logicalPoint({ x: 2, y: 4 }),
        logicalPoint({ x: 1, y: 5 }),
        logicalPoint({ x: 0, y: 6 }),
    ],
    rightPoints: [
        logicalPoint({ x: 4, y: 3 }),
        logicalPoint({ x: 3, y: 4 }),
        logicalPoint({ x: 2, y: 5 }),
        logicalPoint({ x: 1, y: 6 }),
        logicalPoint({ x: 0, y: 7 }),
    ],
});

export const WHITE_CENTER_THROWS = createThrowTestData({
    direction: offset({ x: 0, y: 1 }),
    piece: WHITE_PIECE,
    throwerOrigin: logicalPoint({ x: 5, y: 0 }),
    leftPoints: [
        logicalPoint({ x: 4, y: 2 }),
        logicalPoint({ x: 4, y: 3 }),
        logicalPoint({ x: 4, y: 4 }),
        logicalPoint({ x: 4, y: 5 }),
        logicalPoint({ x: 4, y: 6 }),
        logicalPoint({ x: 4, y: 7 }),
        logicalPoint({ x: 4, y: 8 }),
        logicalPoint({ x: 4, y: 9 }),
    ],
    centerPoints: [
        logicalPoint({ x: 5, y: 2 }),
        logicalPoint({ x: 5, y: 3 }),
        logicalPoint({ x: 5, y: 4 }),
        logicalPoint({ x: 5, y: 5 }),
        logicalPoint({ x: 5, y: 6 }),
        logicalPoint({ x: 5, y: 7 }),
        logicalPoint({ x: 5, y: 8 }),
        logicalPoint({ x: 5, y: 9 }),
    ],
    rightPoints: [
        logicalPoint({ x: 6, y: 2 }),
        logicalPoint({ x: 6, y: 3 }),
        logicalPoint({ x: 6, y: 4 }),
        logicalPoint({ x: 6, y: 5 }),
        logicalPoint({ x: 6, y: 6 }),
        logicalPoint({ x: 6, y: 7 }),
        logicalPoint({ x: 6, y: 8 }),
        logicalPoint({ x: 6, y: 9 }),
    ],
});

export const WHITE_RIGHT_THROWS = createThrowTestData({
    direction: offset({ x: 1, y: 1 }),
    piece: WHITE_PIECE,
    throwerOrigin: logicalPoint({ x: 4, y: 0 }),
    leftPoints: [
        logicalPoint({ x: 6, y: 3 }),
        logicalPoint({ x: 7, y: 4 }),
        logicalPoint({ x: 8, y: 5 }),
        logicalPoint({ x: 9, y: 6 }),
    ],
    centerPoints: [
        logicalPoint({ x: 6, y: 2 }),
        logicalPoint({ x: 7, y: 3 }),
        logicalPoint({ x: 8, y: 4 }),
        logicalPoint({ x: 9, y: 5 }),
    ],
    rightPoints: [
        logicalPoint({ x: 7, y: 2 }),
        logicalPoint({ x: 8, y: 3 }),
        logicalPoint({ x: 9, y: 4 }),
    ],
});

const BLACK_PIECE = createFakePiece({
    position: logicalPoint({ x: 5, y: 8 }),
    color: GameColor.BLACK,
});
export const BLACK_LEFT_THROWS = createThrowTestData({
    direction: offset({ x: -1, y: -1 }),
    piece: BLACK_PIECE,
    throwerOrigin: logicalPoint({ x: 6, y: 9 }),
    leftPoints: [
        logicalPoint({ x: 3, y: 7 }),
        logicalPoint({ x: 2, y: 6 }),
        logicalPoint({ x: 1, y: 5 }),
        logicalPoint({ x: 0, y: 4 }),
    ],
    centerPoints: [
        logicalPoint({ x: 4, y: 7 }),
        logicalPoint({ x: 3, y: 6 }),
        logicalPoint({ x: 2, y: 5 }),
        logicalPoint({ x: 1, y: 4 }),
        logicalPoint({ x: 0, y: 3 }),
    ],
    rightPoints: [
        logicalPoint({ x: 4, y: 6 }),
        logicalPoint({ x: 3, y: 5 }),
        logicalPoint({ x: 2, y: 4 }),
        logicalPoint({ x: 1, y: 3 }),
        logicalPoint({ x: 0, y: 2 }),
    ],
});

export const BLACK_CENTER_THROWS = createThrowTestData({
    direction: offset({ x: 0, y: -1 }),
    piece: BLACK_PIECE,
    throwerOrigin: logicalPoint({ x: 5, y: 9 }),
    leftPoints: [
        logicalPoint({ x: 4, y: 7 }),
        logicalPoint({ x: 4, y: 6 }),
        logicalPoint({ x: 4, y: 5 }),
        logicalPoint({ x: 4, y: 4 }),
        logicalPoint({ x: 4, y: 3 }),
        logicalPoint({ x: 4, y: 2 }),
        logicalPoint({ x: 4, y: 1 }),
        logicalPoint({ x: 4, y: 0 }),
    ],
    centerPoints: [
        logicalPoint({ x: 5, y: 7 }),
        logicalPoint({ x: 5, y: 6 }),
        logicalPoint({ x: 5, y: 5 }),
        logicalPoint({ x: 5, y: 4 }),
        logicalPoint({ x: 5, y: 3 }),
        logicalPoint({ x: 5, y: 2 }),
        logicalPoint({ x: 5, y: 1 }),
        logicalPoint({ x: 5, y: 0 }),
    ],
    rightPoints: [
        logicalPoint({ x: 6, y: 7 }),
        logicalPoint({ x: 6, y: 6 }),
        logicalPoint({ x: 6, y: 5 }),
        logicalPoint({ x: 6, y: 4 }),
        logicalPoint({ x: 6, y: 3 }),
        logicalPoint({ x: 6, y: 2 }),
        logicalPoint({ x: 6, y: 1 }),
        logicalPoint({ x: 6, y: 0 }),
    ],
});

export const BLACK_RIGHT_THROWS = createThrowTestData({
    direction: offset({ x: 1, y: -1 }),
    piece: BLACK_PIECE,
    throwerOrigin: logicalPoint({ x: 4, y: 9 }),
    leftPoints: [
        logicalPoint({ x: 6, y: 6 }),
        logicalPoint({ x: 7, y: 5 }),
        logicalPoint({ x: 8, y: 4 }),
        logicalPoint({ x: 9, y: 3 }),
    ],
    centerPoints: [
        logicalPoint({ x: 6, y: 7 }),
        logicalPoint({ x: 7, y: 6 }),
        logicalPoint({ x: 8, y: 5 }),
        logicalPoint({ x: 9, y: 4 }),
    ],
    rightPoints: [
        logicalPoint({ x: 7, y: 7 }),
        logicalPoint({ x: 8, y: 6 }),
        logicalPoint({ x: 9, y: 5 }),
    ],
});

export function createThrowTestData({
    direction,
    piece,
    throwerOrigin,
    leftPoints,
    centerPoints,
    rightPoints,
}: {
    direction: Offset;
    piece: Piece;
    throwerOrigin: LogicalPoint;
    leftPoints: LogicalPoint[];
    centerPoints: LogicalPoint[];
    rightPoints: LogicalPoint[];
}): ThrowTestData {
    const leftMoves = createTestMoves(throwerOrigin, leftPoints);
    const centerMoves = createTestMoves(throwerOrigin, centerPoints);
    const rightMoves = createTestMoves(throwerOrigin, rightPoints);

    const points: LogicalPoint[][] = [];
    const moves: Move[][] = [];
    if (leftPoints.length > 0) {
        points.push(leftPoints);
        moves.push(leftMoves);
    }
    if (centerPoints.length > 0) {
        points.push(centerPoints);
        moves.push(centerMoves);
    }
    if (rightPoints.length > 0) {
        points.push(rightPoints);
        moves.push(rightMoves);
    }

    const allPoints = [...leftPoints, ...centerPoints, ...rightPoints];
    const allMoves = [...leftMoves, ...centerMoves, ...rightMoves];

    // this needs to be ordered from the left, from the player non flipped perspective
    if (piece.color === GameColor.BLACK) {
        points.reverse();
        moves.reverse();
    }

    return {
        direction,
        throwerOrigin,
        piece,
        moves: {
            sides: moves,
            all: allMoves,
        },
        points: {
            sides: points,
            all: allPoints,
        },
    };
}

function createTestMoves(
    throwerOrigin: LogicalPoint,
    points: LogicalPoint[],
): Move[] {
    return points.map((to) =>
        createFakeMove({
            from: WHITE_PIECE.position,
            to: logicalPoint(to),
            triggers: [throwerOrigin],
        }),
    );
}
