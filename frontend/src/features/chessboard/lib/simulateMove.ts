import {
    AnimationStep,
    MoveAnimation,
    MoveBounds,
    Piece,
    PieceID,
} from "./types";

import BoardPieces from "./boardPieces";
import { Move } from "./types";

export function simulateMove(pieces: BoardPieces, move: Move): AnimationStep {
    return simulateMoveDestination(pieces, move).step;
}

export function simulateMoveWithIntermediates(
    pieces: BoardPieces,
    move: Move,
): MoveAnimation {
    const fromPiece = pieces.getByPosition(move.from);
    if (!fromPiece) return { steps: [] };

    const steps: AnimationStep[] = [];
    const currentPieces = new BoardPieces(pieces);
    currentPieces.removeCapturedPiecesFromMove(move);
    for (const intermediate of move.intermediates) {
        currentPieces.movePiece(fromPiece.id, intermediate.position);

        steps.push({
            newPieces: new BoardPieces(currentPieces),
            movedPieceIds: [fromPiece.id],
            isCapture: intermediate.isCapture,
            specialType: move.specialType,
        });
    }

    const mainMoveAnimation = simulateMoveDestination(pieces, move);
    steps.push(mainMoveAnimation.step);
    return {
        steps,
        removedPieces: mainMoveAnimation.removedPieces,
    };
}

function simulateMoveDestination(
    basePieces: BoardPieces,
    move: Move,
): { step: AnimationStep; removedPieces: Map<PieceID, Piece> } {
    const newPieces = new BoardPieces(basePieces);
    const { movedPieceIds, removedPieces } = newPieces.playMove(move);

    const initialSpawnPositions = createInitialSpawns(basePieces, move);
    const isCapture =
        removedPieces.size > 0 &&
        move.intermediates.filter((x) => x.isCapture).length <
            removedPieces.size;
    const moveBounds: MoveBounds = {
        from: move.from,
        to: move.to,
    };

    return {
        step: {
            newPieces,
            movedPieceIds: [...movedPieceIds],

            initialSpawnPositions,

            moveBounds: moveBounds,
            isCapture,
            isPromotion: move.promotesTo !== null,
            specialType: move.specialType,
        },
        removedPieces,
    };
}

function createInitialSpawns(
    basePieces: BoardPieces,
    move: Move,
): BoardPieces | undefined {
    if (move.pieceSpawns.length === 0) return;

    const initialSpawnPositions = new BoardPieces(basePieces);
    for (const piece of move.pieceSpawns) {
        initialSpawnPositions.addAt(piece, move.from);
    }
    return initialSpawnPositions;
}
