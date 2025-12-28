import { LogicalPoint } from "@/features/point/types";
import { PieceID } from "../lib/types";
import { Move } from "../lib/types";
import { StrPoint } from "@/features/point/types";
import { Piece } from "../lib/types";
import { StateCreator } from "zustand";
import { ChessboardStore } from "./chessboardStore";
import { pointToStr } from "@/features/point/pointUtils";
import { PieceType } from "@/lib/apiClient";
import BoardPieces from "../lib/boardPieces";
import LegalMoves from "../lib/legalMoves";

export interface LegalMovesSliceProps {
    legalMoves: LegalMoves;
}

export interface LegalMovesSlice {
    legalMoves: LegalMoves;
    highlightedLegalMoves: LogicalPoint[];

    getLegalMove(
        dest: LogicalPoint,
        pieceId: PieceID,
        pieces: BoardPieces,
    ): Promise<Move | null>;

    showLegalMoves(piece: Piece): boolean;
    hideLegalMoves(): void;
    flashLegalMoves(): void;

    setLegalMoves(legalMoves: LegalMoves): void;
}

export function createLegalMovesSlice(
    initState: LegalMovesSliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    LegalMovesSlice
> {
    return (set, get) => ({
        ...initState,
        highlightedLegalMoves: [],

        async getLegalMove(dest, pieceId, pieces) {
            const { legalMoves, promptPromotion, disambiguateDestination } =
                get();

            const piece = pieces.getById(pieceId);
            if (!piece) {
                console.warn(
                    `Could not get legal moves for ${pieceId}, no piece was found`,
                );
                return null;
            }

            const movesFromOrigin = legalMoves.get(piece.position);
            if (!movesFromOrigin) return null;

            const movesToDest = await disambiguateDestination(
                dest,
                movesFromOrigin,
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

        showLegalMoves(piece) {
            const { legalMoves } = get();

            const moves = legalMoves.get(piece.position) ?? [];

            const toHighlightPoints = new Map<StrPoint, LogicalPoint>();
            for (const move of moves) {
                if (move.intermediates.length != 0) {
                    toHighlightPoints.set(
                        pointToStr(move.intermediates[0].position),
                        move.intermediates[0].position,
                    );
                    continue;
                }

                for (const trigger of move.triggers) {
                    toHighlightPoints.set(pointToStr(trigger), trigger);
                }
                toHighlightPoints.set(pointToStr(move.to), move.to);
            }

            set((state) => {
                state.highlightedLegalMoves = Array.from(
                    toHighlightPoints.values(),
                );
            });
            return moves.length > 0;
        },

        hideLegalMoves() {
            set((state) => {
                state.highlightedLegalMoves = [];
            });
        },

        flashLegalMoves(): void {
            const { legalMoves, logicalPointToViewPoint, flashOverlay } = get();

            for (const movesPerPoint of legalMoves) {
                for (const move of movesPerPoint) {
                    const from = logicalPointToViewPoint(move.from);
                    const to = logicalPointToViewPoint(move.to);
                    flashOverlay({
                        from: from,
                        to: to,
                        color: "red",
                    });
                }
            }
        },

        setLegalMoves(legalMoves): void {
            set((state) => {
                state.legalMoves = legalMoves;
                state.highlightedLegalMoves = [];
            });
        },
    });
}
