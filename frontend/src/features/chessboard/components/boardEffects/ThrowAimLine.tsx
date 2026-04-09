import { Color, CubicBezierCurve3 } from "three";
import { Line } from "@react-three/drei";

import { PersistentBoardEffectType } from "../../stores/boardEffectsSlice";
import { useChessboardStore } from "../../hooks/useChessboard";
import { viewToWorld } from "@/features/point/pointUtils";
import { LogicalPoint } from "@/features/point/types";

export interface ThrowAimEffect {
    type: PersistentBoardEffectType.THROW_AIM_LINE;
    from: LogicalPoint;
    mid: LogicalPoint;
    to: LogicalPoint;
}

export const DASH_SIZE = 0.2;
export const GAP_SIZE = 0.1;

const ThrowAimLine = ({ effect }: { effect: ThrowAimEffect }) => {
    const fromView = useChessboardStore((x) =>
        x.logicalPointToViewPoint(effect.from),
    );
    const midView = useChessboardStore((x) =>
        x.logicalPointToViewPoint(effect.mid),
    );
    const toView = useChessboardStore((x) =>
        x.logicalPointToViewPoint(effect.to),
    );

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

    const points = curve.getPoints(20);

    return (
        <Line
            points={points}
            color="#ff9e44"
            lineWidth={8}
            dashed
            dashSize={DASH_SIZE}
            gapSize={GAP_SIZE}
            vertexColors={points.map((_, i) => {
                const alpha = 0.2 + 0.7 * (i / (points.length - 1));
                return new Color(1, 0.2, 0).multiplyScalar(alpha);
            })}
        />
    );
};
export default ThrowAimLine;
