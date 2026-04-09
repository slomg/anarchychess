import { decodeFen } from "../../../chessboard/lib/fenDecoder";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import { logicalPoint } from "@/features/point/pointUtils";
import { GameColor, PieceType } from "@/lib/apiClient";
import createDefaultChessboard from "../defaultBoard";
import BoardPieces from "../boardPieces";
import constants from "@/lib/constants";

describe("decodeFen", () => {
    it("should parse a standard starting position correctly", () => {
        mockSequentialUUID();
        const board = decodeFen(constants.INITIAL_FEN);
        expect(board).toEqual(createDefaultChessboard());
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
            },
            {
                id: "1",
                position: logicalPoint({ x: 4, y: 3 }),
                type: PieceType.ROOK,
                color: GameColor.WHITE,
                stunnedForTurns: 0,
            },
            {
                id: "2",
                position: logicalPoint({ x: 3, y: 4 }),
                type: PieceType.BISHOP,
                color: GameColor.WHITE,
                stunnedForTurns: 0,
            },
            {
                id: "3",
                position: logicalPoint({ x: 4, y: 7 }),
                type: PieceType.KING,
                color: GameColor.BLACK,
                stunnedForTurns: 0,
            },
        );
        const board = decodeFen(fen);
        expect(board).toEqual(expectedBoard);
    });
});
