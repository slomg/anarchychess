import { useEffect } from "react";

import { useChessboardStore } from "../../hooks/useChessboard";

const BoardEffectsWebGLFallback = () => {
    const transientEffects = useChessboardStore(
        (x) => x.activeTransientBoardEffects,
    );
    useEffect(() => {
        for (const effect of transientEffects.values()) {
            effect.complete();
        }
    }, [transientEffects]);

    return null;
};
export default BoardEffectsWebGLFallback;
