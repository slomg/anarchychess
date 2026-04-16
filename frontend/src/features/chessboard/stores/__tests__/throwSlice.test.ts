import { StoreApi } from "zustand";

import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import {
    createFakeMove,
    createFakePiece,
    createRandomPoint,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { logicalPoint } from "@/features/point/pointUtils";
import { PendingThrow } from "../throwSlice";

describe("ThrowSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    describe("promptThrow", () => {
        it("should setup pending throw state and resolve to move", async () => {
            const throwerOrigin = logicalPoint({ x: 5, y: 0 });
            const piece = createFakePiece({
                position: logicalPoint({ x: 5, y: 1 }),
            });
            const moves = Array.from({ length: 10 }).map(() =>
                createFakeMove({ from: piece.position }),
            );

            const promise = store
                .getState()
                .promptThrow(throwerOrigin, piece, moves);

            const pendingThrow = store.getState().pendingThrow;
            expect(pendingThrow).toEqual<PendingThrow>({
                piece,
                points: moves.map((x) => x.to),
                throwerOrigin,

                resolve: expect.any(Function),
            });

            pendingThrow?.resolve(moves[0].to);

            const result = await promise;

            expect(result).toEqual(moves[0]);
        });

        it("should resolve to null when pending throw is resolved with null", async () => {
            const throwerOrigin = logicalPoint({ x: 5, y: 0 });
            const piece = createFakePiece({
                position: logicalPoint({ x: 5, y: 1 }),
            });
            const moves = Array.from({ length: 10 }).map(() =>
                createFakeMove({ from: piece.position }),
            );

            const promise = store
                .getState()
                .promptThrow(throwerOrigin, piece, moves);

            store.getState().pendingThrow?.resolve(null);

            const result = await promise;

            expect(result).toBeNull();
        });

        it("should unselect piece", () => {
            const unselectPieceMock = vi.fn();
            store.setState({ unselectPiece: unselectPieceMock });

            store
                .getState()
                .promptThrow(createRandomPoint(), createFakePiece(), []);

            expect(unselectPieceMock).toHaveBeenCalledExactlyOnceWith();
        });
    });
});
