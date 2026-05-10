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

import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import PositionHistory from "../../lib/positionHistory";
import { PositionId } from "../../lib/position";
import { GameColor } from "@/lib/apiClient";

describe("CoreSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        vi.useFakeTimers();
        store = createChessboardStore();
    });

    describe("resetState", () => {
        let newChessboardState: ChessboardProps;

        beforeEach(() => {
            newChessboardState = {
                viewingFrom: GameColor.BLACK,
                pieces: createFakeBoardPieces(),
                legalMovesByPosition: new Map([
                    [
                        "asd" as PositionId,
                        createFakeLegalMoves({ hasForcedMoves: true }),
                    ],
                ]),
                disableDrag: true,
                muteAudio: true,
            };
        });

        it("should reset the state to props", async () => {
            await store.getState().resetState(newChessboardState);

            const state = store.getState();
            expect(state).toEqual({
                ...store.getInitialState(),
                ...newChessboardState,
            });
        });

        it("should animate pieces from the old position if viewing a position", async () => {
            const updatePiecesFromPositionMock = vi.fn();
            store.setState({
                updatePiecesFromPosition: updatePiecesFromPositionMock,
            });

            newChessboardState.positionHistory = new PositionHistory({
                pieces: createFakeBoardPieces(),
            });
            const latestPosition =
                newChessboardState.positionHistory.addNextPosition(
                    createFakePositionProps({
                        pieces: newChessboardState.pieces,
                    }),
                );

            await store.getState().resetState(newChessboardState);

            expect(updatePiecesFromPositionMock).toHaveBeenCalledWith(
                latestPosition,
            );
        });

        it("should not call updatePiecesFromPosition if not viewing a position", async () => {
            const updatePiecesFromPositionMock = vi.fn();
            store.setState({
                updatePiecesFromPosition: updatePiecesFromPositionMock,
            });

            newChessboardState.positionHistory = new PositionHistory({
                pieces: newChessboardState.pieces,
            });

            await store.getState().resetState(newChessboardState);

            expect(updatePiecesFromPositionMock).not.toHaveBeenCalled();
        });
    });
});
