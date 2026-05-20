import WebGL from "three/examples/jsm/capabilities/WebGL.js";
import { Canvas } from "@react-three/fiber";

import BoardEffectsWebGLFallback from "./BoardEffectsWebGLFallback";
import BoardEffects from "./BoardEffects";

const BoardEffectsCanvas = () => {
    const test = Canvas;
    return WebGL.isWebGL2Available() ? (
        <Canvas
            className="pointer-events-none! absolute! inset-0 z-40 touch-none
                select-none"
            data-testid="boardEffects"
        >
            <BoardEffects />
        </Canvas>
    ) : (
        <BoardEffectsWebGLFallback />
    );
};
export default BoardEffectsCanvas;
