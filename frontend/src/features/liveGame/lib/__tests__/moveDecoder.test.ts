import brotliCompress from "brotli/compress";

import {
    ForcedMovePriority,
    GameColor,
    MovePath,
    PieceType,
    SpecialMoveType,
} from "@/lib/apiClient";
import { decodeLegalMoves, decodeMovePathIntoLegalMoves } from "../moveDecoder";
import { Move, MoveKey } from "@/features/chessboard/lib/types";
import { logicalPoint } from "@/features/point/pointUtils";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";

vi.mock("brotli/compress");

const emptyMove = {
    triggers: [],
    captures: [],
    intermediates: [],
    sideEffects: [],
    pieceSpawns: [],
    promotesTo: null,
    specialType: SpecialMoveType.NONE,
    forcedPriority: ForcedMovePriority.NONE,
    highlightSquare: false,
};

describe("decodeMovePathIntoLegalMoves", () => {
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
                promotesTo: PieceType.BISHOP,
                specialType: SpecialMoveType.EN_PASSANT,
                forcedPriority: ForcedMovePriority.UNDERAGE_PAWN,
                highlightSquare: true,
            },
        ];

        mockSequentialUUID();
        const result = decodeMovePathIntoLegalMoves({
            paths,
            boardWidth: 10,
        });

        expect(result.size).toBe(1);
        const moves = result.get(logicalPoint({ x: 0, y: 0 }));
        expect(moves).toBeDefined();
        expect(moves).toHaveLength(1);

        const move = moves![0];
        expect(move).toEqual<Move>({
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
                },
            ],
            promotesTo: PieceType.BISHOP,
            specialType: SpecialMoveType.EN_PASSANT,
            forcedPriority: ForcedMovePriority.UNDERAGE_PAWN,
            highlightSquare: true,
        });
    });

    it("should group multiple moves from the same fromIdx", () => {
        const paths: MovePath[] = [
            { fromIdx: 0, toIdx: 1, moveKey: "2" },
            { fromIdx: 0, toIdx: 2, moveKey: "3" },
        ];
        const boardWidth = 10;

        const result = decodeMovePathIntoLegalMoves({
            paths,
            boardWidth,
        });

        expect(result.size).toBe(1);
        const moves = result.get(logicalPoint({ x: 0, y: 0 }));
        expect(moves).toEqual([
            {
                ...emptyMove,
                from: { x: 0, y: 0 },
                to: { x: 1, y: 0 },
                moveKey: "2",
            },
            {
                ...emptyMove,
                from: { x: 0, y: 0 },
                to: { x: 2, y: 0 },
                moveKey: "3",
            },
        ]);
    });

    it("should return empty map when paths is empty", () => {
        const result = decodeMovePathIntoLegalMoves({
            paths: [],
            boardWidth: 10,
        });
        expect(result).toEqual(new LegalMoves());
    });

    it("should set hasForcedMoves when at least one move has forcedPriority", () => {
        const paths: MovePath[] = [
            createFakeMovePath({ forcedPriority: ForcedMovePriority.NONE }),
            createFakeMovePath({
                forcedPriority: ForcedMovePriority.UNDERAGE_PAWN,
            }),
        ];

        const result = decodeMovePathIntoLegalMoves({
            paths,
            boardWidth: 10,
        });

        expect(result.hasForcedMoves).toBe(true);
    });

    it("should collect highlightSquares from moves with highlightSquare = true", () => {
        const paths: MovePath[] = [
            createFakeMovePath({ fromIdx: 0, highlightSquare: true }),
            createFakeMovePath({ fromIdx: 2, highlightSquare: false }),
            createFakeMovePath({ fromIdx: 4, highlightSquare: true }),
        ];

        const result = decodeMovePathIntoLegalMoves({
            paths,
            boardWidth: 10,
        });

        expect(result.highlightSquares).toEqual([
            logicalPoint({ x: 0, y: 0 }),
            logicalPoint({ x: 4, y: 0 }),
        ]);
    });
});

describe("decodeLegalMoves", () => {
    it("should decode a valid base64 gzipped encoded move string", () => {
        const moves: MovePath[] = [
            {
                fromIdx: 0,
                toIdx: 1,
                triggerIdxs: [2],
                capturedIdxs: [3],
                intermediateSquares: [{ posIdx: 4, isCapture: false }],
                sideEffects: [{ fromIdx: 5, toIdx: 6 }],
                pieceSpawns: [
                    {
                        type: PieceType.CHECKER,
                        color: GameColor.BLACK,
                        posIdx: 7,
                    },
                ],
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
        const result = decodeLegalMoves({
            encoded,
            boardWidth: 10,
        });

        expect(result.size).toBe(2);
        expect(result.get(logicalPoint({ x: 0, y: 0 }))).toEqual<Move[]>([
            {
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 1, y: 0 }),
                moveKey: "1" as MoveKey,
                triggers: [logicalPoint({ x: 2, y: 0 })],
                captures: [logicalPoint({ x: 3, y: 0 })],
                intermediates: [
                    {
                        position: logicalPoint({ x: 4, y: 0 }),
                        isCapture: false,
                    },
                ],
                sideEffects: [
                    {
                        from: logicalPoint({ x: 5, y: 0 }),
                        to: logicalPoint({ x: 6, y: 0 }),
                    },
                ],
                pieceSpawns: [
                    {
                        id: "0",
                        type: PieceType.CHECKER,
                        color: GameColor.BLACK,
                        position: logicalPoint({ x: 7, y: 0 }),
                    },
                ],
                promotesTo: null,
                specialType: SpecialMoveType.NONE,
                forcedPriority: ForcedMovePriority.NONE,
                highlightSquare: false,
            },
        ]);
        expect(result.get(logicalPoint({ x: 0, y: 1 }))).toEqual<Move[]>([
            {
                from: logicalPoint({ x: 0, y: 1 }),
                to: logicalPoint({ x: 1, y: 1 }),
                moveKey: "5" as MoveKey,
                ...emptyMove,
            },
        ]);
    });

    it("should return empty map when given encoded empty move list", () => {
        const compressed = brotliCompress(Buffer.from("[]"));
        const encoded = Buffer.from(compressed).toString("base64");

        const result = decodeLegalMoves({
            encoded,
            boardWidth: 10,
        });
        expect(result).toEqual(new LegalMoves());
    });

    it("should return empty map when given an empty string", () => {
        const result = decodeLegalMoves({
            encoded: "",
            boardWidth: 10,
        });
        expect(result).toEqual(new LegalMoves([], false));
    });
});
