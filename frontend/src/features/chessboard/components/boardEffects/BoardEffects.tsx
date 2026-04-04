import { Canvas } from "@react-three/fiber";
import { JSX } from "react";

import { useChessboardStore } from "../../hooks/useChessboard";
import type { ThrowAimEffect } from "./ThrowAimLine";
import ThrowAimLine from "./ThrowAimLine";

export enum BoardEffectType {
    THROW_AIM_LINE,
}

export type BoardEffect = ThrowAimEffect;

const BoardEffects = () => {
    const effects = useChessboardStore((x) => x.activeBoardEffects);

    const result: JSX.Element[] = [];
    for (const [id, effect] of effects) {
        switch (effect.type) {
            case BoardEffectType.THROW_AIM_LINE:
                result.push(<ThrowAimLine effect={effect} key={id} />);
                break;
        }
    }

    return (
        <Canvas
            className="pointer-events-none! absolute! inset-0 z-40 touch-none
                select-none"
            data-testid="boardEffects"
        >
            {result}
        </Canvas>
    );
};
export default BoardEffects;
