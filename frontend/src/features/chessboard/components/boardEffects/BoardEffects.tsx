import { JSX } from "react";

import {
    PersistentBoardEffectType,
    TransientBoardEffectType,
} from "../../stores/boardEffectsSlice";

import { viewPoint, viewToWorld } from "@/features/point/pointUtils";
import { useChessboardStore } from "../../hooks/useChessboard";
import ThrowAimLineEffect from "./ThrowAimLineEffect";
import PawnThrowEffect from "./PawnThrowEffect";
import ExplosionEffect from "./ExplosionEffect";
import QueentumTunnelingEffect from "./QueentumTunnelingEffect";

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
                result.push(<ThrowAimLineEffect effect={effect} key={id} />);
                break;
        }
    }

    for (const [id, effect] of transientEffects) {
        switch (effect.value.type) {
            case TransientBoardEffectType.PAWN_THROW:
                result.push(
                    <PawnThrowEffect
                        effect={effect.value}
                        onSettle={effect.settle}
                        onComplete={effect.complete}
                        key={id}
                    />,
                );
                break;
            case TransientBoardEffectType.EXPLOSION:
                result.push(
                    <ExplosionEffect
                        effect={effect.value}
                        onSettle={effect.settle}
                        onComplete={effect.complete}
                        key={id}
                    />,
                );
                break;
            case TransientBoardEffectType.QUEENTUM_TUNNELLING:
                result.push(
                    <QueentumTunnelingEffect
                        effect={effect.value}
                        onSettle={effect.settle}
                        onComplete={effect.complete}
                        key={id}
                    />,
                );
                break;
        }
    }

    return (
        <>
            <ambientLight intensity={1} />
            <directionalLight
                position={viewToWorld(viewPoint({ x: 9, y: 9 })).toArray()}
            />

            {result}
        </>
    );
};
export default BoardEffects;
