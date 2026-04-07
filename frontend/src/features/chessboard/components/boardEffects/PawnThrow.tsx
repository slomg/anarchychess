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
    color: GameColor;
}

export const BLACK_PAWN_COLOR = new Color("#575452");
const UP = new Vector3(0, 1, 0);

const PawnThrow = ({
    effect,
    onFinish,
}: {
    effect: PawnThrowEffect;
    onFinish: () => void;
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
    const boardDimensions = useChessboardStore((x) => x.boardDimensions);

    const from = viewToWorld(fromView);
    const to = viewToWorld(toView);
    const mid = new Vector3().addVectors(from, to).multiplyScalar(0.5);

    if (toView.x <= 3) {
        mid.x += 2;
    } else if (toView.x >= boardDimensions.width - 3) {
        mid.x -= 2;
    }

    if (toView.y <= 3) {
        mid.y -= 2;
    } else if (toView.y >= boardDimensions.height - 3) {
        mid.y += 2;
    }

    mid.z += 3;
    const curve = new QuadraticBezierCurve3(from, mid, to);

    const meshRef = useRef<Object3D>(null);

    const [hasFinishedThrow, setHasFinishedThrow] = useState(false);

    const flip = useRef(0);
    const spin = useRef(0);
    const progressRef = useRef(0);
    const curveLength = curve.getLength();
    useFrame((_, delta) => {
        if (!meshRef.current || hasFinishedThrow) {
            return;
        }

        const time = progressRef.current + (10 * delta) / curveLength;
        if (time > 1) {
            setHasFinishedThrow(true);
            return;
        }
        progressRef.current = time;

        const position = curve.getPoint(time);
        const forward = curve.getTangent(time).normalize();
        meshRef.current.position.copy(position);

        const lookAt = position.clone().add(forward);
        const baseQuat = new Quaternion().setFromRotationMatrix(
            new Matrix4().lookAt(position, lookAt, UP),
        );
        const correctionQuat = new Quaternion().setFromEuler(
            new Euler(Math.PI / 2, 0, 0),
        );

        const rotationFactor = 1 - time;

        // flip forward
        const axis = new Vector3().crossVectors(UP, forward).normalize();
        flip.current += delta * 5 * rotationFactor;
        const flipQuat = new Quaternion().setFromAxisAngle(axis, flip.current);

        // spin
        spin.current += delta * 10 * rotationFactor;
        const spinQuat = new Quaternion().setFromAxisAngle(UP, spin.current);

        meshRef.current.quaternion
            .copy(baseQuat)
            .multiply(correctionQuat)
            .multiply(flipQuat)
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
                    <primitive object={colored} scale={0.15} />
                    <primitive object={hull} scale={0.15} />
                </group>
            )}

            {hasFinishedThrow && (
                <SpriteAnimator
                    spriteDataset={explosionSprite}
                    fps={30}
                    position={to}
                    scale={1.5}
                    onEnd={onFinish}
                />
            )}
        </>
    );
};
export default PawnThrow;
