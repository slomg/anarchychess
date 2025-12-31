import { StoreApi } from "zustand";
import {
    ChessboardProps,
    ChessboardStore,
    createChessboardStore,
} from "../chessboardStore";
import {
    createFakeLegalMoves,
    createFakeBoardPieces,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { GameColor } from "@/lib/apiClient";
import { LogicalPoint } from "@/features/point/types";
import { PieceID } from "../../lib/types";
import { logicalPoint } from "@/features/point/pointUtils";
import LegalMoves from "../../lib/legalMoves";
import { PositionId } from "../../lib/positionHistory";

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

    describe("disableMovement", () => {
        it("should clear latest legal moves, highlightedLegalMoves, and selectedPieceId", () => {
            const selectedPieceId: PieceID = "0";
            const legalMoves = createFakeLegalMoves({ hasForcedMoves: true });
            const highlightedLegalMoves: LogicalPoint[] = [
                logicalPoint({ x: 1, y: 2 }),
                logicalPoint({ x: 2, y: 3 }),
            ];

            const { positionHistory, disableMovement, setLatestLegalMoves } =
                store.getState();
            store.setState({
                highlightedLegalMoves,
                selectedPieceId,
            });
            setLatestLegalMoves(legalMoves);

            disableMovement();

            const state = store.getState();

            expect(
                state.legalMovesByPosition.get(
                    positionHistory.viewingPosition?.positionId,
                ),
            ).toEqual(new LegalMoves());
            expect(state.highlightedLegalMoves).toEqual([]);
            expect(state.selectedPieceId).toBeNull();
        });
    });
});
