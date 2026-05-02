import { SpriteAnimator, useSpriteLoader } from "@react-three/drei";

import { TransientBoardEffectType } from "../../stores/boardEffectsSlice";
import { useChessboardStore } from "../../hooks/useChessboard";
import { viewToWorld } from "@/features/point/pointUtils";
import { LogicalPoint } from "@/features/point/types";
import constants from "@/lib/constants";

export interface ExplosionEffect {
    type: TransientBoardEffectType.EXPLOSION;
    at: LogicalPoint;
}

const ExplosionEffect = ({
    effect,
    onSettle,
    onComplete,
}: {
    effect: ExplosionEffect;
    onSettle: () => void;
    onComplete: () => void;
}) => {
    const { spriteObj: explosionSprite } = useSpriteLoader(
        constants.SPRITE_SHEETS.EXPLOSION,
        null,
        null,
        22,
    );
    const at = viewToWorld(
        useChessboardStore((x) => x.logicalPointToViewPoint(effect.at)),
    );

    return (
        <SpriteAnimator
            spriteDataset={explosionSprite}
            fps={30}
            position={at}
            scale={1.5}
            onStart={onSettle}
            onEnd={onComplete}
        />
    );
};
export default ExplosionEffect;
