import brotliCompress from "brotli/compress";

import {
    ForcedMovePriority,
    GameColor,
    MovePath,
    PieceType,
    SpecialMoveType,
} from "@/lib/apiClient";

import { decodeLegalMoves, decodeMovePathIntoLegalMoves } from "../moveDecoder";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import { Move, MoveKey } from "@/features/chessboard/lib/types";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { logicalPoint } from "@/features/point/pointUtils";

vi.mock("brotli/compress");

const emptyMove = {
    triggers: [],
    captures: [],
    intermediates: [],
    sideEffects: [],
    pieceSpawns: [],
    stuns: [],
    promotesTo: null,
    specialType: SpecialMoveType.NONE,
    forcedPriority: ForcedMovePriority.NONE,
    emphasizeSquare: false,
    overtimeRemovals: [],
};

describe("decodeMovePathIntoLegalMoves", () => {
    const addMoveSpy = vi.spyOn(LegalMoves.prototype, "addMove");

    it("should decode single path into correct LegalMoveMap entry", () => {
        const paths: MovePath[] = [
            {
                fromIdx: 0,
                toIdx: 1,
                moveKey: "2",
                triggerIdxs: [3],
                capturedIdxs: [4],
                intermediateSquares: [{ posIdx: 5, isCapture: true }],
                sideEffects: [{ fromIdx: 6, toIdx: 7 }],
                pieceSpawns: [
                    {
                        type: PieceType.CHECKER,
                        color: GameColor.BLACK,
                        posIdx: 8,
                    },
                ],
                stuns: [{ posIdx: 9, stunForTurns: 5 }],
                promotesTo: PieceType.BISHOP,
                specialType: SpecialMoveType.EN_PASSANT,
                forcedPriority: ForcedMovePriority.UNDERAGE_PAWN,
                emphasizeSquare: true,
                overtimeRemovalIdxs: [10],
            },
        ];

        mockSequentialUUID();
        decodeMovePathIntoLegalMoves(paths);

        expect(addMoveSpy).toHaveBeenCalledExactlyOnceWith<[Move]>({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 1, y: 0 }),
            moveKey: "2" as MoveKey,
            triggers: [logicalPoint({ x: 3, y: 0 })],
            captures: [logicalPoint({ x: 4, y: 0 })],
            intermediates: [
                { position: logicalPoint({ x: 5, y: 0 }), isCapture: true },
            ],
            sideEffects: [
                {
                    from: logicalPoint({ x: 6, y: 0 }),
                    to: logicalPoint({ x: 7, y: 0 }),
                },
            ],
            pieceSpawns: [
                {
                    id: "0",
                    type: PieceType.CHECKER,
                    color: GameColor.BLACK,
                    position: logicalPoint({ x: 8, y: 0 }),
                    stunnedForTurns: 0,
                    hasMoved: false,
                },
            ],
            stuns: [
                { position: logicalPoint({ x: 9, y: 0 }), stunForTurns: 5 },
            ],
            promotesTo: PieceType.BISHOP,
            specialType: SpecialMoveType.EN_PASSANT,
            forcedPriority: ForcedMovePriority.UNDERAGE_PAWN,
            emphasizeSquare: true,
            overtimeRemovals: [logicalPoint({ x: 0, y: 1 })],
        });
    });

    it("should group multiple moves from the same fromIdx", () => {
        const paths: MovePath[] = [
            { fromIdx: 0, toIdx: 1, moveKey: "2" },
            { fromIdx: 0, toIdx: 2, moveKey: "3" },
        ];

        decodeMovePathIntoLegalMoves(paths);

        expect(addMoveSpy).toBeCalledTimes(2);
        expect(addMoveSpy).toHaveBeenCalledWith<Move[]>({
            ...emptyMove,
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 1, y: 0 }),
            moveKey: "2" as MoveKey,
        });
        expect(addMoveSpy).toHaveBeenCalledWith<Move[]>({
            ...emptyMove,
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 2, y: 0 }),
            moveKey: "3" as MoveKey,
        });
    });

    it("should return empty map when paths is empty", () => {
        const result = decodeMovePathIntoLegalMoves([]);
        expect(result).toEqual(new LegalMoves());
    });
});

describe("decodeLegalMoves", () => {
    const addMoveSpy = vi.spyOn(LegalMoves.prototype, "addMove");

    it("should decode a valid base64 gzipped encoded move string", () => {
        const moves: MovePath[] = [
            {
                fromIdx: 0,
                toIdx: 1,
                moveKey: "1",
            },
            {
                fromIdx: 10,
                toIdx: 11,
                moveKey: "5",
            },
        ];

        const jsonString = JSON.stringify(moves);
        const compressed = brotliCompress(Buffer.from(jsonString));
        const encoded = Buffer.from(compressed).toString("base64");

        mockSequentialUUID();
        decodeLegalMoves(encoded);

        expect(addMoveSpy).toHaveBeenCalledTimes(2);
        expect(addMoveSpy).toHaveBeenCalledWith<[Move]>({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 1, y: 0 }),
            moveKey: "1" as MoveKey,
            ...emptyMove,
        });
        expect(addMoveSpy).toHaveBeenCalledWith<[Move]>({
            from: logicalPoint({ x: 0, y: 1 }),
            to: logicalPoint({ x: 1, y: 1 }),
            moveKey: "5" as MoveKey,
            ...emptyMove,
        });
    });

    it("should return empty map when given encoded empty move list", () => {
        const compressed = brotliCompress(Buffer.from("[]"));
        const encoded = Buffer.from(compressed).toString("base64");

        const result = decodeLegalMoves(encoded);
        expect(result).toEqual(new LegalMoves());
    });

    it("should return empty map when given an empty string", () => {
        const result = decodeLegalMoves("");
        expect(result).toEqual(new LegalMoves());
    });
});
