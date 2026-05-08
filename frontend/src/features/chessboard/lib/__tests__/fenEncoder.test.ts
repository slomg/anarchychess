import { logicalPoint } from "@/features/point/pointUtils";
import { GameColor, PieceType } from "@/lib/apiClient";
import createDefaultChessboard from "../defaultBoard";
import { encodeFen } from "../fenEncoder";
import { decodeFen } from "../fenDecoder";
import BoardPieces from "../boardPieces";
import constants from "@/lib/constants";

describe("encodeFen", () => {
    it("should encode the initial position", () => {
        const fen = encodeFen({
            pieces: createDefaultChessboard(),
            sideToMove: GameColor.WHITE,
        });
        expect(fen).toBe(constants.INITIAL_FEN);
    });

    it("should encode a position with no special piece state", () => {
        const pieces = BoardPieces.fromPieces(
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
                position: logicalPoint({ x: 4, y: 9 }),
                type: PieceType.KING,
                color: GameColor.BLACK,
                stunnedForTurns: 0,
                hasMoved: false,
            },
        );
        const fen = encodeFen({ pieces, sideToMove: GameColor.WHITE });
        expect(fen).toBe(`4k5/10/10/10/10/10/10/10/10/4K5`);
    });

    it("should encode moved pieces", () => {
        const pieces = BoardPieces.fromPieces(
            {
                id: "0",
                position: logicalPoint({ x: 4, y: 0 }),
                type: PieceType.KING,
                color: GameColor.WHITE,
                stunnedForTurns: 0,
                hasMoved: true,
            },
            {
                id: "1",
                position: logicalPoint({ x: 4, y: 9 }),
                type: PieceType.KING,
                color: GameColor.BLACK,
                stunnedForTurns: 0,
                hasMoved: false,
            },
        );
        const fen = encodeFen({ pieces, sideToMove: GameColor.WHITE });
        expect(fen).toBe(
            `4k5/10/10/10/10/10/10/10/10/4K5 {"movedPieces":["e1"]}`,
        );
    });

    it("should encode stunned pieces", () => {
        const pieces = BoardPieces.fromPieces(
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
                position: logicalPoint({ x: 4, y: 9 }),
                type: PieceType.KING,
                color: GameColor.BLACK,
                stunnedForTurns: 2,
                hasMoved: false,
            },
        );
        const fen = encodeFen({ pieces, sideToMove: GameColor.WHITE });
        expect(fen).toBe(
            `4k5/10/10/10/10/10/10/10/10/4K5 {"stunnedPieces":{"e10":2}}`,
        );
    });

    it("should encode sideToMove as BLACK", () => {
        const pieces = BoardPieces.fromPieces(
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
                position: logicalPoint({ x: 4, y: 9 }),
                type: PieceType.KING,
                color: GameColor.BLACK,
                stunnedForTurns: 0,
                hasMoved: false,
            },
        );
        const fen = encodeFen({ pieces, sideToMove: GameColor.BLACK });
        expect(fen).toBe(`4k5/10/10/10/10/10/10/10/10/4K5 {"sideToMove":1}`);
    });

    it("should round trip with decodeFen", () => {
        const fen = `4k5/10/10/10/10/10/10/10/10/4K5 {"sideToMove":1,"movedPieces":["e10"],"stunnedPieces":{"e1":3}}`;
        const decoded = decodeFen(fen);
        const reencoded = encodeFen(decoded);
        expect(reencoded).toBe(fen);
    });
});
