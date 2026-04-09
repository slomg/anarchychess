import { TransientBoardEffectType } from "../stores/boardEffectsSlice";
import { AnimationStep, MoveBounds, Piece, PieceID } from "./types";
import { SpecialMoveType } from "@/lib/apiClient";
import BoardPieces from "./boardPieces";
import { Move } from "./types";

type SpecialMoveAnimationHandler = (
    basePieces: BoardPieces,
    move: Move,
    fromPiece: Piece,
) => AnimationStep[];

interface SpecialMoveAnimation {
    handler: SpecialMoveAnimationHandler;
    alreadyPlayedLocally: boolean;
}

interface SimulateMoveAnimatedOptions {
    skipAlreadyPlayedLocally?: boolean;
}

export function simulateMoveAnimated(
    pieces: BoardPieces,
    move: Move,
    { skipAlreadyPlayedLocally }: SimulateMoveAnimatedOptions = {},
): AnimationStep[] {
    skipAlreadyPlayedLocally ??= false;

    const fromPiece = pieces.getByPosition(move.from);
    if (!fromPiece) {
        return [];
    }

    const steps: AnimationStep[] = [];

    if (!skipAlreadyPlayedLocally) {
        steps.push(...animateIntermediates(pieces, move, fromPiece));
    }

    const specialHandler = specialMoveAnimationHandlers.get(move.specialType);
    const shouldPlaySpecialHandler =
        !skipAlreadyPlayedLocally || !specialHandler?.alreadyPlayedLocally;
    if (specialHandler && shouldPlaySpecialHandler) {
        steps.push(...specialHandler.handler(pieces, move, fromPiece));
    }

    const mainMoveStep = simulateMove(pieces, move);
    steps.push(mainMoveStep);
    return steps;
}

function animateIntermediates(
    pieces: BoardPieces,
    move: Move,
    fromPiece: Piece,
): AnimationStep[] {
    const steps: AnimationStep[] = [];
    const currentPieces = new BoardPieces(pieces);
    const intermediateFadedPieces =
        currentPieces.removeRemovedPiecesFromMove(move);
    for (const intermediate of move.intermediates) {
        currentPieces.movePiece(fromPiece.id, intermediate.position);

        steps.push({
            newPieces: new BoardPieces(currentPieces),
            movedPieceIds: [fromPiece.id],
            fadedPieces: intermediateFadedPieces,
            isCapture: intermediate.isCapture,
            specialType: move.specialType,
        });
    }

    return steps;
}

const specialMoveAnimationHandlers = new Map<
    SpecialMoveType,
    SpecialMoveAnimation
>([
    [
        SpecialMoveType.THROW,
        { handler: simulateThrowMove, alreadyPlayedLocally: false },
    ],
]);

function simulateThrowMove(
    basePieces: BoardPieces,
    move: Move,
    fromPiece: Piece,
): AnimationStep[] {
    const newPieces = new BoardPieces(basePieces);
    newPieces.removeFrom(move.from);

    return [
        {
            newPieces,
            movedPieceIds: [],

            boardEffect: {
                type: TransientBoardEffectType.PAWN_THROW,
                from: move.from,
                to: move.to,
                color: fromPiece.color,
            },
            disableStepDelay: true,
        },
    ];
}

export function simulateMove(
    basePieces: BoardPieces,
    move: Move,
): AnimationStep {
    const newPieces = new BoardPieces(basePieces);
    const overtimeRemovals = getOvertimeRemovals(move, newPieces);

    const movedPieceIds = newPieces.playMove(move);

    const initialSpawnPositions = createInitialSpawns(basePieces, move);
    const isCapture =
        move.captures &&
        move.captures.length > 0 &&
        move.intermediates.filter((x) => x.isCapture).length <
            move.captures.length;
    const moveBounds: MoveBounds = {
        from: move.from,
        to: move.to,
    };

    return {
        newPieces,
        movedPieceIds: [...movedPieceIds],

        initialSpawnPositions,
        fadedPieces: overtimeRemovals,

        moveBounds: moveBounds,
        isCapture,
        isPromotion: move.promotesTo !== null,
        specialType: move.specialType,
    };
}

function getOvertimeRemovals(
    move: Move,
    pieces: BoardPieces,
): Map<PieceID, Piece> | undefined {
    if (move.overtimeRemovals.length === 0) {
        return;
    }

    const overtimeRemovals = new Map<PieceID, Piece>();
    for (const pos of move.overtimeRemovals) {
        const piece = pieces.getByPosition(pos);
        if (piece) overtimeRemovals.set(piece.id, piece);
    }
    return overtimeRemovals;
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
