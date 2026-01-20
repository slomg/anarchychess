import { StateCreator } from "zustand";

import { pointToStr } from "@/features/point/pointUtils";
import { LogicalPoint } from "@/features/point/types";
import { ChessboardStore } from "./chessboardStore";
import { StrPoint } from "@/features/point/types";
import BoardPieces from "../lib/boardPieces";
import { PositionId } from "../lib/position";
import { PieceType } from "@/lib/apiClient";
import { PieceID } from "../lib/types";
import { Piece } from "../lib/types";
import { Move } from "../lib/types";

export interface UiLegalMovesSliceProps {
    hideLegalMoves?: boolean;
}

export interface UiLegalMovesSlice {
    highlightedLegalMoves: LogicalPoint[];
    hideLegalMoves: boolean;

    hasLegalMovesForPosition(positionId?: PositionId): boolean;
    setHideLegalMoves(value: boolean): void;

    getLegalMove(
        dest: LogicalPoint,
        pieceId: PieceID,
        pieces: BoardPieces,
    ): Promise<Move | null>;

    highlightLegalMoves(piece: Piece): boolean;
    unhighlightLegalMoves(): void;
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
            if (!moveNode) return null;

            const movesToDest = await disambiguateIntermediates(
                dest,
                moveNode,
                pieceId,
                pieces,
            );
            if (movesToDest.length === 0) return null;

            if (movesToDest.length === 1) return movesToDest[0];

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

        highlightLegalMoves(piece) {
            const { getViewedPositionLegalMoves } = get();

            const legalMoves = getViewedPositionLegalMoves();
            const moveNodes = legalMoves.getFromOrigin(piece.position);

            const toHighlightPoints = new Map<StrPoint, LogicalPoint>();
            for (const moveNode of moveNodes) {
                for (const move of moveNode.terminalMoves) {
                    for (const trigger of move.triggers) {
                        toHighlightPoints.set(pointToStr(trigger), trigger);
                    }

                    if (move.intermediates.length != 0) {
                        toHighlightPoints.set(
                            pointToStr(move.intermediates[0].position),
                            move.intermediates[0].position,
                        );
                        continue;
                    }
                }

                toHighlightPoints.set(pointToStr(moveNode.at), moveNode.at);
            }

            set((state) => {
                state.highlightedLegalMoves = Array.from(
                    toHighlightPoints.values(),
                );
            });
            return toHighlightPoints.size > 0;
        },

        unhighlightLegalMoves() {
            set((state) => {
                state.highlightedLegalMoves = [];
            });
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
