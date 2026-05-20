import WebGL from "three/examples/jsm/capabilities/WebGL.js";
import { render } from "@testing-library/react";

import BoardEffectsWebGLFallback from "../BoardEffectsWebGLFallback";
import BoardEffectsCanvas from "../BoardEffectsCanvas";
import BoardEffects from "../BoardEffects";

vi.mock("../BoardEffectsWebGLFallback");
vi.mock("../BoardEffects");
vi.mock("@react-three/fiber", () => ({
    Canvas: ({ children }: { children: React.ReactNode }) => children,
}));

describe("BoardEffectsCanvas", () => {
    it("should render board effects when webgl is available", () => {
        vi.spyOn(WebGL, "isWebGL2Available").mockReturnValue(true);

        render(<BoardEffectsCanvas />);

        expect(BoardEffects).toHaveBeenCalled();
        expect(BoardEffectsWebGLFallback).not.toHaveBeenCalled();
    });

    it("should render webgl fallback when webgl is not available", () => {
        vi.spyOn(WebGL, "isWebGL2Available").mockReturnValue(false);

        render(<BoardEffectsCanvas />);

        expect(BoardEffectsWebGLFallback).toHaveBeenCalled();
        expect(BoardEffects).not.toHaveBeenCalled();
    });
});
