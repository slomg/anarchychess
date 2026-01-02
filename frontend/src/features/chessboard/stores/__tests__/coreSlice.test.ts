import { StoreApi } from "zustand";

import {
    createFakeLegalMoves,
    createFakeBoardPieces,
} from "@/lib/testUtils/fakers/chessboardFakers";
import {
    ChessboardProps,
    ChessboardStore,
    createChessboardStore,
} from "../chessboardStore";

import { PositionId } from "../../lib/position";
import { GameColor } from "@/lib/apiClient";

describe("CoreSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        vi.useFakeTimers();
        store = createChessboardStore();
    });

    describe("resetState", () => {
        it("should reset the state to props", () => {
            const newChessboardState: ChessboardProps = {
                viewingFrom: GameColor.BLACK,
                boardDimensions: {
                    width: 6,
                    height: 9,
                },
                pieces: createFakeBoardPieces(),
                legalMovesByPosition: new Map([
                    [
                        "asd" as PositionId,
                        createFakeLegalMoves({ hasForcedMoves: true }),
                    ],
                ]),
                canDrag: false,
                muteAudio: true,
            };

            store.getState().resetState(newChessboardState);

            const state = store.getState();
            expect(state).toEqual({
                ...store.getInitialState(),
                ...newChessboardState,
            });
        });
    });
});
