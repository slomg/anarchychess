import { Canvas } from "@react-three/fiber";
import { JSX } from "react";

import { viewPoint, viewToWorld } from "@/features/point/pointUtils";

import { useChessboardStore } from "../../hooks/useChessboard";
import ThrowAimLine from "./ThrowAimLine";
import PawnThrow from "./PawnThrow";
import {
    PersistentBoardEffectType,
    TransientBoardEffectType,
} from "../../stores/boardEffectsSlice";

const BoardEffects = () => {
    const persistentEffects = useChessboardStore(
        (x) => x.activePersistentBoardEffects,
    );
    const transientEffects = useChessboardStore(
        (x) => x.activeTransientBoardEffects,
    );

    const result: JSX.Element[] = [];
    for (const [id, effect] of persistentEffects) {
        switch (effect.type) {
            case PersistentBoardEffectType.THROW_AIM_LINE:
                result.push(<ThrowAimLine effect={effect} key={id} />);
                break;
        }
    }

    for (const [id, effect] of transientEffects) {
        switch (effect.value.type) {
            case TransientBoardEffectType.PAWN_THROW:
                result.push(
                    <PawnThrow
                        effect={effect.value}
                        onFinish={effect.finish}
                        key={id}
                    />,
                );
                break;
        }
    }

    return (
        <Canvas
            className="pointer-events-none! absolute! inset-0 z-40 touch-none
                select-none"
            data-testid="boardEffects"
        >
            <ambientLight intensity={1} />
            <directionalLight
                position={viewToWorld(viewPoint({ x: 9, y: 9 }))}
            />

            {result}
        </Canvas>
    );
};
export default BoardEffects;
