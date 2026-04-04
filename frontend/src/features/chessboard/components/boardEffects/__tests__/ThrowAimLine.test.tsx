import { render } from "@testing-library/react";
import { CubicBezierCurve3 } from "three";
import { Line } from "@react-three/drei";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import ThrowAimLine, {
    DASH_SIZE,
    GAP_SIZE,
    ThrowAimEffect,
} from "../ThrowAimLine";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { logicalPoint, viewToWorld } from "@/features/point/pointUtils";
import { BoardEffectType } from "../BoardEffects";

vi.mock("@react-three/drei", () => ({
    Line: vi.fn(),
}));

describe("ThrowAimLine", () => {
    let store: StoreApi<ChessboardStore>;

    const lineMock = vi.mocked(Line);

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should create a curve from 'from' to 'mid' to 'to'", () => {
        const effect: ThrowAimEffect = {
            type: BoardEffectType.THROW_AIM_LINE,
            from: logicalPoint({ x: 1, y: 1 }),
            mid: logicalPoint({ x: 3, y: 3 }),
            to: logicalPoint({ x: 5, y: 2 }),
        };

        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowAimLine effect={effect} />
            </ChessboardStoreContext.Provider>,
        );

        const logicalPointToViewPoint =
            store.getState().logicalPointToViewPoint;
        const fromView = logicalPointToViewPoint(effect.from);
        const midView = logicalPointToViewPoint(effect.mid);
        const toView = logicalPointToViewPoint(effect.to);

        const from = viewToWorld(fromView);
        const mid = viewToWorld(midView);
        const to = viewToWorld(toView);

        let curve = new CubicBezierCurve3(from, mid, mid, to);

        const pattern = DASH_SIZE + GAP_SIZE;
        const length = curve.getLength();
        const dashRemainder = length % pattern;
        if (dashRemainder !== 0) {
            const dir = to.clone().sub(mid).normalize();
            const extendedTo = to
                .clone()
                .add(dir.multiplyScalar(pattern - dashRemainder));
            curve = new CubicBezierCurve3(from, mid, mid, extendedTo);
        }

        expect(lineMock).toHaveBeenCalledWith(
            expect.objectContaining({
                points: curve.getPoints(20),
                dashed: true,
                dashSize: DASH_SIZE,
                gapSize: GAP_SIZE,
            }),
            undefined,
        );
    });
});
