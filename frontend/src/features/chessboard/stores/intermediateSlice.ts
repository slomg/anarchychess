import { StateCreator } from "zustand";

import { pointEquals, pointToStr } from "@/features/point/pointUtils";
import type { ChessboardStore } from "./chessboardStore";
import { LogicalPoint } from "@/features/point/types";
import { Move, PieceID } from "../lib/types";
import BoardPieces from "../lib/boardPieces";
import { MoveNode } from "../lib/legalMoves";

export interface IntermediateSlice {
    nextIntermediates: LogicalPoint[];
    intermediateVisited: LogicalPoint[];

    resolveNextIntermediate: ((move: LogicalPoint | null) => void) | null;
    disambiguateIntermediates(
        dest: LogicalPoint,
        moveNode: MoveNode,
        pieceId: PieceID,
        pieces: BoardPieces,
    ): Promise<Move[]>;
}

export const createIntermediateSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    IntermediateSlice
> = (set, get) => ({
    nextIntermediates: [],
    intermediateVisited: [],
    resolveNextIntermediate: null,

    async disambiguateIntermediates(dest, moveNode, pieceId, pieces) {
        const { animatePiece } = get();
        try {
            while (true) {
                const movesThatEndHere = moveNode.terminalMoves;
                const nextIntermediates = moveNode.nextIntermediates;

                if (nextIntermediates.size === 0) {
                    return movesThatEndHere;
                }

                animatePiece(pieceId, dest, pieces);

                const nextOptions: LogicalPoint[] = [
                    ...nextIntermediates.values(),
                ].map((x) => x.at);
                if (movesThatEndHere.length > 0) {
                    nextOptions.push(dest);
                }
                const choice = await new Promise<LogicalPoint | null>(
                    (resolve) => {
                        set((state) => {
                            state.nextIntermediates = nextOptions;
                            state.resolveNextIntermediate = resolve;
                        });
                    },
                );

                // move was cancelled
                if (!choice) return [];

                // if we click on the same square
                // return all moves that end in this destination
                if (pointEquals(choice, dest)) {
                    return movesThatEndHere;
                }

                dest = choice;
                const nextNode = nextIntermediates.get(pointToStr(dest));
                if (!nextNode) return [];

                moveNode = nextNode;
            }
        } finally {
            set((state) => {
                state.nextIntermediates = [];
                state.resolveNextIntermediate = null;
            });
        }
    },
});
