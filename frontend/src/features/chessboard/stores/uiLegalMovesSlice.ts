import { StateCreator } from "zustand";

import { PieceType, SpecialMoveType } from "@/lib/apiClient";
import { LogicalPoint } from "@/features/point/types";
import { ChessboardStore } from "./chessboardStore";
import BoardPieces from "../lib/boardPieces";
import { PositionId } from "../lib/position";
import { PieceID } from "../lib/types";
import { Move } from "../lib/types";

export interface UiLegalMovesSliceProps {
    hideLegalMoves?: boolean;
}

export interface UiLegalMovesSlice {
    hideLegalMoves: boolean;

    hasLegalMovesForPosition(positionId?: PositionId): boolean;
    setHideLegalMoves(value: boolean): void;

    getLegalMove(
        dest: LogicalPoint,
        pieceId: PieceID,
        pieces: BoardPieces,
    ): Promise<Move | null>;
    flashLegalMoves(): void;
}

export function createUiLegalMovesSlice(
    initState: UiLegalMovesSliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    UiLegalMovesSlice
> {
    return (set, get) => ({
        ...initState,
        hideLegalMoves: initState.hideLegalMoves ?? false,
        highlightedLegalMoves: [],

        setHideLegalMoves(value) {
            set((state) => {
                state.hideLegalMoves = value;
            });
        },

        hasLegalMovesForPosition(positionId) {
            const { legalMovesByPosition } = get();
            return legalMovesByPosition.has(positionId);
        },

        async getLegalMove(dest, pieceId, pieces) {
            const {
                getViewedPositionLegalMoves,
                disambiguateIntermediates,
                promptPromotion,
                promptThrow,
            } = get();

            const piece = pieces.getById(pieceId);
            if (!piece) {
                console.warn(
                    `Could not get legal moves for ${pieceId}, no piece was found`,
                );
                return null;
            }

            const legalMoves = getViewedPositionLegalMoves();
            const moveNode = legalMoves.getDirectNode(piece.position, dest);
            if (!moveNode) {
                return null;
            }

            const movesToDest = await disambiguateIntermediates(
                dest,
                moveNode,
                pieceId,
                pieces,
            );
            if (movesToDest.length === 0) {
                return null;
            }

            if (movesToDest.length === 1) {
                return movesToDest[0];
            }

            if (movesToDest[0].specialType === SpecialMoveType.THROW) {
                const throwResult = await promptThrow(dest, piece, movesToDest);
                return throwResult;
            }

            // multiple moves to the same destination, must be a promotion
            const availablePromotions = new Map<PieceType | null, Move>();
            for (const move of movesToDest) {
                availablePromotions.set(move.promotesTo, move);
            }

            const promoteTo = await promptPromotion({
                at: dest,
                pieces: [...availablePromotions.keys()],
                piece,
            });
            return availablePromotions.get(promoteTo) ?? null;
        },

        flashLegalMoves() {
            const {
                getViewedPositionLegalMoves,
                logicalPointToViewPoint,
                flashOverlay,
            } = get();

            const legalMoves = getViewedPositionLegalMoves();
            for (const from of legalMoves.byOrigin.values()) {
                for (const nodes of from.values()) {
                    const from = logicalPointToViewPoint(nodes.from);
                    const to = logicalPointToViewPoint(nodes.at);
                    flashOverlay({
                        from,
                        to,
                        color: "red",
                    });
                }
            }
        },
    });
}
