import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import getEffectivePieceColor from "../effectivePieceColor";
import { logicalPoint } from "@/features/point/pointUtils";
import { GameColor, PieceType } from "@/lib/apiClient";
import BoardPieces from "../boardPieces";

describe("getEffectivePieceColor", () => {
    it.each([GameColor.WHITE, GameColor.BLACK])(
        "should return the piece color for normal pieces",
        (color) => {
            const piece = createFakePiece({ color });
            const pieces = BoardPieces.fromPieces(piece);

            expect(getEffectivePieceColor(piece, pieces)).toBe(color);
        },
    );

    describe("Traitor Rook", () => {
        it("should return null if there are no adjacent pieces", () => {
            const piece = createFakePiece({
                type: PieceType.TRAITOR_ROOK,
                color: null,
                position: logicalPoint({ x: 5, y: 5 }),
            });
            const pieces = BoardPieces.fromPieces(
                piece,
                createFakePiece({ position: logicalPoint({ x: 1, y: 2 }) }),
            );

            expect(getEffectivePieceColor(piece, pieces)).toBeNull();
        });

        it("should return null if there is no color majority", () => {
            const piece = createFakePiece({
                type: PieceType.TRAITOR_ROOK,
                color: null,
                position: logicalPoint({ x: 5, y: 5 }),
            });
            const whiteAdjacent = createFakePiece({
                position: logicalPoint({ x: 4, y: 5 }),
                color: GameColor.WHITE,
            });
            const blackAdjacent = createFakePiece({
                position: logicalPoint({ x: 6, y: 5 }),
                color: GameColor.BLACK,
            });
            const pieces = BoardPieces.fromPieces(
                whiteAdjacent,
                piece,
                blackAdjacent,
            );

            expect(getEffectivePieceColor(piece, pieces)).toBeNull();
        });

        it.each([
            [GameColor.WHITE, GameColor.BLACK],
            [GameColor.BLACK, GameColor.WHITE],
        ])(
            "should return the correct color for majority",
            (majorityColor, minorityColor) => {
                const piece = createFakePiece({
                    type: PieceType.TRAITOR_ROOK,
                    color: null,
                    position: logicalPoint({ x: 5, y: 5 }),
                });
                const majorityAdjacent1 = createFakePiece({
                    position: logicalPoint({ x: 4, y: 5 }),
                    color: majorityColor,
                });
                const majorityAdjacent2 = createFakePiece({
                    position: logicalPoint({ x: 5, y: 6 }),
                    color: majorityColor,
                });
                const minorityAdjacent = createFakePiece({
                    position: logicalPoint({ x: 6, y: 5 }),
                    color: minorityColor,
                });
                const pieces = BoardPieces.fromPieces(
                    majorityAdjacent1,
                    majorityAdjacent2,
                    piece,
                    minorityAdjacent,
                );

                expect(getEffectivePieceColor(piece, pieces)).toBe(
                    majorityColor,
                );
            },
        );

        it("sould not count stunned pieces", () => {
            const piece = createFakePiece({
                type: PieceType.TRAITOR_ROOK,
                color: null,
                position: logicalPoint({ x: 5, y: 5 }),
            });
            const whiteStunned = createFakePiece({
                position: logicalPoint({ x: 4, y: 5 }),
                color: GameColor.WHITE,
                stunnedForTurns: 1,
            });
            const blackAdjacent = createFakePiece({
                position: logicalPoint({ x: 6, y: 5 }),
                color: GameColor.BLACK,
            });

            const pieces = BoardPieces.fromPieces(
                whiteStunned,
                piece,
                blackAdjacent,
            );

            expect(getEffectivePieceColor(piece, pieces)).toBe(GameColor.BLACK);
        });
    });
});
