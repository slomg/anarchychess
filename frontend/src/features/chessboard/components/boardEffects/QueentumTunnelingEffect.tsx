import {
    Color,
    NormalBlending,
    ShaderMaterial,
    Texture,
    TextureLoader,
    Vector2,
} from "three";

import { useFrame } from "@react-three/fiber";
import { useMemo, useRef } from "react";

import { TransientBoardEffectType } from "../../stores/boardEffectsSlice";
import { LogicalPoint } from "@/features/point/types";
import { useTexture } from "@react-three/drei";
import { getPieceImage } from "../../lib/pieceImage";
import { GameColor, PieceType } from "@/lib/apiClient";

export interface QueentumTunnelingEffect {
    type: TransientBoardEffectType.QUEENTUM_TUNNELLING;
    queenPosition: LogicalPoint;
    antiqueenPosition: LogicalPoint;
}
interface WarpUniforms {
    uSquareSize: { value: number };
    uColorA: { value: Color };
    uColorB: { value: Color };
    uAlpha: { value: number };

    uCenter: { value: Vector2 };
    uRadius: { value: number };
    uStrength: { value: number };

    uTexture: { value: Texture };
    uTextureSize: { value: Vector2 };
    uTextureAlpha: { value: number };
}

const QueentumTunnelingEffect = ({
    effect,
    onSettle,
    onComplete,
}: {
    effect: QueentumTunnelingEffect;
    onSettle: () => void;
    onComplete: () => void;
}) => {
    const texture = useTexture(getPieceImage(PieceType.QUEEN, GameColor.WHITE));
    const uniforms = useMemo<WarpUniforms>(
        () => ({
            uSquareSize: { value: 0.77 },
            uColorA: { value: new Color("#577298").convertLinearToSRGB() },
            uColorB: { value: new Color("#e9e9d4").convertLinearToSRGB() },
            uAlpha: { value: 1 },

            uCenter: { value: new Vector2(0.77 / 2, 0.77 / 2) },
            uRadius: { value: 0.775 },
            uStrength: { value: 0 },

            uTexture: { value: texture },
            uTextureSize: { value: new Vector2(0.77, 0.75) },
            uTextureAlpha: { value: 1 },
        }),
        [texture],
    );
    const materialRef = useRef<ShaderMaterial & { uniforms: WarpUniforms }>(
        null,
    );

    function cubicBezier(
        t: number,
        p0: number,
        p1: number,
        p2: number,
        p3: number,
    ) {
        const u = 1 - t;
        return (
            u * u * u * p0 +
            3 * u * u * t * p1 +
            3 * u * t * t * p2 +
            t * t * t * p3
        );
    }

    const progressRef = useRef(0);
    useFrame((state, delta) => {
        if (!materialRef.current) {
            return;
        }
        progressRef.current += delta;
        const time = progressRef.current * 1.2;

        materialRef.current.uniforms.uStrength.value = cubicBezier(
            time,
            0,
            1,
            -3,
            0.5,
        );

        materialRef.current.uniforms.uTextureAlpha.value =
            1 - cubicBezier(time, 1, -1, 3, 1);

        let alpha = 1;

        if (time < 0.2) {
            alpha = time / 0.2;
        } else if (time > 0.85) {
            alpha = (1 - time) / 0.15;
        } else {
            alpha = 1;
        }

        materialRef.current.uniforms.uAlpha.value = alpha;

        if (time >= 1) {
            onComplete();
        }
    });

    return (
        <mesh>
            <planeGeometry args={[0.775 * 10, 0.775 * 10]} />
            <shaderMaterial
                ref={materialRef}
                transparent
                depthWrite={false}
                depthTest={false}
                blending={NormalBlending}
                uniforms={
                    uniforms as unknown as Record<string, { value: unknown }>
                }
                vertexShader={`
                    varying vec2 vWorldPos;
                    void main() {
                        vec4 worldPos = modelMatrix * vec4(position, 1.0);
                        vWorldPos = worldPos.xy;
                        gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
                    }
                `}
                fragmentShader={`
                    uniform float uSquareSize;
                    uniform vec3 uColorA;
                    uniform vec3 uColorB;
                    uniform float uAlpha;

                    uniform vec2 uCenter;
                    uniform float uRadius;
                    uniform float uStrength;
                    varying vec2 vWorldPos;

                    uniform sampler2D uTexture;
                    uniform vec2 uTextureSize;
                    uniform float uTextureAlpha;

                    void main() {
                        vec2 toCenter = uCenter - vWorldPos;
                        float dist = length(toCenter);
                        
                        float warp = (1.0 - smoothstep(0.0, uRadius, dist)) * uStrength;
                        vec2 warpedPos = vWorldPos - toCenter * warp;

                        vec2 baseUV = (vWorldPos - uCenter) / uTextureSize + 0.5;
                        vec2 warpOffset = warpedPos - vWorldPos;
                        vec2 texUV = baseUV - warpOffset / uTextureSize;
                        float inBounds = step(0.0, texUV.x) * step(texUV.x, 1.0)
                                    * step(0.0, texUV.y) * step(texUV.y, 1.0);
                        vec4 texColor = texture2D(uTexture, texUV) * inBounds;
                        texColor.a *= uTextureAlpha;

                        vec2 sq = warpedPos / uSquareSize;
                        vec2 grid = abs(fract(sq) - 0.5);
                        vec2 fw = fwidth(sq);
                        vec2 edgeBlend = smoothstep(0.5 - fw, 0.5 + fw, grid);
                        float checker = mod(floor(sq.x) + floor(sq.y), 2.0);
                        float blend = max(edgeBlend.x, edgeBlend.y);
                        vec3 checkerColor = mix(
                            mix(uColorA, uColorB, checker),
                            mix(uColorB, uColorA, checker),
                            blend
                        );

                        vec3 color = mix(checkerColor, texColor.rgb, texColor.a);

                        vec2 rectDist = abs(vWorldPos - uCenter) / uRadius;
                        float rectAlpha = smoothstep(1.0, 0.7, max(rectDist.x, rectDist.y));
                        gl_FragColor = vec4(color, rectAlpha * uAlpha);
                    }
                `}
            />
        </mesh>
    );
};
export default QueentumTunnelingEffect;
