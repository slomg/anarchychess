import {
    Color,
    Euler,
    Matrix4,
    Object3D,
    QuadraticBezierCurve3,
    Quaternion,
    Vector3,
} from "three";

import { SpriteAnimator, useGLTF, useSpriteLoader } from "@react-three/drei";
import { useFrame } from "@react-three/fiber";
import { useRef, useState } from "react";

import { TransientBoardEffectType } from "../../stores/boardEffectsSlice";
import replaceSceneColor from "@/features/graphics/lib/replaceSceneColor";
import { useChessboardStore } from "../../hooks/useChessboard";
import inverseHull from "@/features/graphics/lib/inverseHull";
import { viewToWorld } from "@/features/point/pointUtils";
import { LogicalPoint } from "@/features/point/types";
import { GameColor } from "@/lib/apiClient";
import constants from "@/lib/constants";

export interface PawnThrowEffect {
    type: TransientBoardEffectType.PAWN_THROW;
    from: LogicalPoint;
    to: LogicalPoint;
    color: GameColor | null;
}

export const BLACK_PAWN_COLOR = new Color("#575452");
const UP = new Vector3(0, 1, 0);
const THROW_SPEED = 7;
const SPIN_SPEED = 5;

const PawnThrowEffect = ({
    effect,
    onSettle,
    onComplete,
}: {
    effect: PawnThrowEffect;
    onSettle: () => void;
    onComplete: () => void;
}) => {
    const { scene } = useGLTF(constants.MODELS.PAWN);
    const { spriteObj: explosionSprite } = useSpriteLoader(
        constants.SPRITE_SHEETS.EXPLOSION,
        null,
        null,
        22,
    );

    const fromView = useChessboardStore((x) =>
        x.logicalPointToViewPoint(effect.from),
    );
    const toView = useChessboardStore((x) =>
        x.logicalPointToViewPoint(effect.to),
    );

    const from = viewToWorld(fromView);
    const to = viewToWorld(toView);
    const mid = new Vector3().addVectors(from, to).multiplyScalar(0.5);

    if (toView.x <= 3) {
        mid.x += 2;
    } else if (toView.x >= constants.BOARD_WIDTH - 3) {
        mid.x -= 2;
    }

    if (toView.y <= 3) {
        mid.y -= 2;
    } else if (toView.y >= constants.BOARD_HEIGHT - 3) {
        mid.y += 2;
    }

    mid.z += 3;
    const curve = new QuadraticBezierCurve3(from, mid, to);

    const meshRef = useRef<Object3D>(null);

    const [hasFinishedThrow, setHasFinishedThrow] = useState(false);

    const spin = useRef(0);
    const progressRef = useRef(0);
    const curveLength = curve.getLength();
    useFrame((_, delta) => {
        if (!meshRef.current || hasFinishedThrow) {
            return;
        }

        const time = progressRef.current + (delta * THROW_SPEED) / curveLength;
        if (time > 1) {
            setHasFinishedThrow(true);
            onSettle();
            return;
        }
        progressRef.current = time;

        const position = curve.getPoint(time);
        const forward = curve.getTangent(time).multiplyScalar(-1).normalize();
        meshRef.current.position.copy(position);

        const lookAt = position.clone().add(forward);
        const baseQuat = new Quaternion().setFromRotationMatrix(
            new Matrix4().lookAt(position, lookAt, UP),
        );
        const correctionQuat = new Quaternion().setFromEuler(
            new Euler(Math.PI / 2, 0, 0),
        );

        // spin
        spin.current += delta * SPIN_SPEED;
        const spinQuat = new Quaternion().setFromAxisAngle(UP, spin.current);

        meshRef.current.quaternion
            .copy(baseQuat)
            .multiply(correctionQuat)
            .multiply(spinQuat);
    });

    const colored =
        effect.color === GameColor.WHITE
            ? scene.clone()
            : replaceSceneColor(scene, new Color("white"), BLACK_PAWN_COLOR);
    const hull = inverseHull(scene);

    return (
        <>
            {!hasFinishedThrow && (
                <group ref={meshRef}>
                    <primitive object={colored} scale={0.13} />
                    <primitive object={hull} scale={0.13} />
                </group>
            )}

            {hasFinishedThrow && (
                <SpriteAnimator
                    spriteDataset={explosionSprite}
                    fps={30}
                    position={to}
                    scale={1.5}
                    onEnd={onComplete}
                />
            )}
        </>
    );
};
export default PawnThrowEffect;
