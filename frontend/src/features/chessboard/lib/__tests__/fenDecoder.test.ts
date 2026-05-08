import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import { decodeFen } from "../../../chessboard/lib/fenDecoder";
import { logicalPoint } from "@/features/point/pointUtils";
import { GameColor, PieceType } from "@/lib/apiClient";
import createDefaultChessboard from "../defaultBoard";
import BoardPieces from "../boardPieces";
import constants from "@/lib/constants";

describe("decodeFen", () => {
    it("should parse a standard starting position correctly", () => {
        mockSequentialUUID();
        const { pieces, sideToMove } = decodeFen(constants.INITIAL_FEN);
        expect(pieces).toEqual(createDefaultChessboard());
        expect(sideToMove).toBe(GameColor.WHITE);
    });

    it("should parse a custom position", () => {
        mockSequentialUUID();

        const fen = "4k3/8/8/3B4/4R3/8/8/4K3";
        const expectedBoard = BoardPieces.fromPieces(
            {
                id: "0",
                position: logicalPoint({ x: 4, y: 0 }),
                type: PieceType.KING,
                color: GameColor.WHITE,
                stunnedForTurns: 0,
                hasMoved: false,
            },
            {
                id: "1",
                position: logicalPoint({ x: 4, y: 3 }),
                type: PieceType.ROOK,
                color: GameColor.WHITE,
                stunnedForTurns: 0,
                hasMoved: false,
            },
            {
                id: "2",
                position: logicalPoint({ x: 3, y: 4 }),
                type: PieceType.BISHOP,
                color: GameColor.WHITE,
                stunnedForTurns: 0,
                hasMoved: false,
            },
            {
                id: "3",
                position: logicalPoint({ x: 4, y: 7 }),
                type: PieceType.KING,
                color: GameColor.BLACK,
                stunnedForTurns: 0,
                hasMoved: false,
            },
        );
        const { pieces, sideToMove } = decodeFen(fen);
        expect(pieces).toEqual(expectedBoard);
        expect(sideToMove).toBe(GameColor.WHITE);
    });

    it("should parse stunned pieces correctly", () => {
        mockSequentialUUID();
        const fen = `4k5/10/10/10/10/10/10/10/10/4K5 {"stunnedPieces":{"e10":2}}`;
        const { pieces } = decodeFen(fen);
        const blackKing = pieces.getByPosition(logicalPoint({ x: 4, y: 9 }));
        expect(blackKing?.stunnedForTurns).toBe(2);
    });

    it("should parse moved pieces correctly", () => {
        mockSequentialUUID();
        const fen = `4k5/10/10/10/10/10/10/10/10/4K5 {"movedPieces":["e1"]}`;
        const { pieces } = decodeFen(fen);
        const whiteKing = pieces.getByPosition(logicalPoint({ x: 4, y: 0 }));
        expect(whiteKing?.hasMoved).toBe(true);
    });

    it.each([GameColor.WHITE, GameColor.BLACK])(
        "should parse sideToMove correctly",
        (expectedSideToMove) => {
            mockSequentialUUID();
            const fen = `4k5/10/10/10/10/10/10/10/10/4K5 {"sideToMove":${expectedSideToMove}}`;
            const { sideToMove } = decodeFen(fen);
            expect(sideToMove).toBe(expectedSideToMove);
        },
    );
});
