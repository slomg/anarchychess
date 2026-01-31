import {
    ForcedMovePriority,
    GameColor,
    PieceType,
    SpecialMoveType,
} from "@/lib/apiClient";
import { LogicalPoint } from "@/features/point/types";
import BoardPieces from "./boardPieces";

export interface Piece {
    id: PieceID;
    type: PieceType;
    color: GameColor | null;
    position: LogicalPoint;
}

export interface Move {
    from: LogicalPoint;
    to: LogicalPoint;
    moveKey: MoveKey;

    triggers: LogicalPoint[];
    captures: LogicalPoint[];
    intermediates: IntermediateSquare[];
    sideEffects: MoveSideEffect[];
    pieceSpawns: Piece[];
    promotesTo: PieceType | null;
    forcedPriority: ForcedMovePriority;
    specialType: SpecialMoveType;
    emphasizeSquare: boolean;

    overtimeRemovals: LogicalPoint[];
}

export type MoveKey = string & { __brand: "MoveKey" };

export type MinimalMove = Partial<Move> & {
    from: LogicalPoint;
    to: LogicalPoint;
};

export interface MoveSideEffect {
    from: LogicalPoint;
    to: LogicalPoint;
}

export interface IntermediateSquare {
    position: LogicalPoint;
    isCapture: boolean;
}

export type PieceID = string;

export interface MoveBounds {
    from: LogicalPoint;
    to: LogicalPoint;
}

export interface AnimationStep {
    newPieces: BoardPieces;
    movedPieceIds: PieceID[];

    initialSpawnPositions?: BoardPieces;
    fadedPieces?: Map<PieceID, Piece>;

    moveBounds?: MoveBounds;
    specialType?: SpecialMoveType | null;
    isCapture?: boolean;
    isPromotion?: boolean;
    hasOvertimeRemovals?: boolean;
}

export interface GameReplay {
    _comment?: string;
    startingFen: string;
    moves: MinimalMove[];
}
