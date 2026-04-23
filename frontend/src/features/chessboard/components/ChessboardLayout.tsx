import { useEffect, useLayoutEffect, useMemo, useRef, useState } from "react";
import { useGLTF, useSpriteLoader } from "@react-three/drei";
import { Canvas } from "@react-three/fiber";
import { twMerge } from "tailwind-merge";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import HighlightedLegalMovesRenderer from "./HighlightedLegalMove";
import IntermediateSquarePrompt from "./IntermediateSquarePrompt";
import EmphasizedSquaresRenderer from "./EmphasizedSquare";
import BoardEffects from "./boardEffects/BoardEffects";
import LastMoveHighlight from "./LastMoveHighlight";
import OverlayRenderer from "./OverlayRenderer";
import PromotionPrompt from "./PromotionPrompt";
import PieceRenderer from "./PieceRenderer";
import constants from "@/lib/constants";
import ThrowPrompt from "./ThrowPrompt";
import Coords from "./Coords";

export interface PaddingOffset {
    width: number;
    height: number;
    maxSize?: number;
}

export interface ChessboardBreakpoint {
    maxScreenSize: number;
    paddingOffset: PaddingOffset;
}

export interface ChessboardLayoutProps {
    breakpoints?: ChessboardBreakpoint[];
    defaultOffset?: PaddingOffset;
    className?: string;
    children?: React.ReactNode;
}

if (typeof window !== "undefined") {
    useGLTF.preload(constants.MODELS.PAWN);
    useSpriteLoader.preload(constants.SPRITE_SHEETS.EXPLOSION);
    AudioPlayer.preload(
        AudioType.MOVE,
        AudioType.CAPTURE,
        AudioType.ILLEGAL_MOVE,
        AudioType.PROMOTION,
        AudioType.EXPLOSION,
        AudioType.CASTLE,
    );
}

const ChessboardLayout = ({
    breakpoints = [],
    defaultOffset,
    className,
    children,
}: ChessboardLayoutProps) => {
    const [boardSize, setBoardSize] = useState<number>(0);
    const { setBoardRect, onPointerDown, onPointerUp, disableDrag } =
        useChessboardStore((x) => ({
            setBoardRect: x.setBoardRect,
            onPointerDown: x.onPointerDown,
            onPointerUp: x.onPointerUp,
            disableDrag: x.disableDrag,
        }));
    const boardDimensions = useChessboardStore((x) => x.boardDimensions);

    const ref = useRef<HTMLDivElement>(null);

    // Sort the offset breakpoints in ascending order
    const sortedBreakpoints = useMemo(
        () => breakpoints.sort((a, b) => a.maxScreenSize - b.maxScreenSize),
        [breakpoints],
    );

    useEffect(() => {
        /**
         * Calculate the width and height offset based on the offsetBreakpoints param and window width
         */
        function calculateOffset(): PaddingOffset {
            const width = window.innerWidth;
            for (const { maxScreenSize, paddingOffset } of sortedBreakpoints) {
                if (maxScreenSize > width) return paddingOffset;
            }

            return (
                defaultOffset ?? {
                    width: 0,
                    height: 0,
                }
            );
        }

        /**
         * Set the board size based on the viewport size and the offset
         */
        function resizeBoard(): void {
            const {
                width: offsetWidth,
                height: offsetHeight,
                maxSize,
            } = calculateOffset();

            const width = window.innerWidth - offsetWidth;
            const height = window.innerHeight - offsetHeight;

            let minSize = Math.max(264, Math.min(width, height));
            if (maxSize !== undefined) minSize = Math.min(minSize, maxSize);
            setBoardSize(minSize);
        }

        window.addEventListener("resize", resizeBoard);
        resizeBoard();

        return () => window.removeEventListener("resize", resizeBoard);
    }, [defaultOffset, sortedBreakpoints]);

    useLayoutEffect(() => {
        let timeoutId: NodeJS.Timeout;
        function updateRect() {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(() => {
                if (ref.current) {
                    setBoardRect(ref.current.getBoundingClientRect());
                }
            }, 100);
        }

        if (ref.current) {
            setBoardRect(ref.current.getBoundingClientRect());
        }

        window.addEventListener("scroll", updateRect);
        window.addEventListener("resize", updateRect);

        return () => {
            window.removeEventListener("scroll", updateRect);
            window.removeEventListener("resize", updateRect);
        };
    }, [boardSize, ref, setBoardRect]);

    return (
        <div
            data-testid="chessboard"
            className={twMerge(
                "relative cursor-pointer select-none",
                !disableDrag && "touch-none",
                className,
            )}
            style={{ width: `${boardSize}px`, height: `${boardSize}px` }}
            ref={ref}
            onPointerDown={onPointerDown}
            onPointerUp={onPointerUp}
            onContextMenu={(e) => e.preventDefault()}
        >
            <svg
                viewBox={`0 0 ${boardDimensions.width} ${boardDimensions.height}`}
                preserveAspectRatio="none"
                className="absolute inset-0 h-full w-full rounded-md"
                shapeRendering="crispEdges"
            >
                <rect
                    x="0"
                    y="0"
                    width={boardDimensions.width}
                    height={boardDimensions.height}
                    fill="#577298"
                />
                <path
                    fill="#e9e9d4"
                    d="
                        M 0,0 H 10 v 1 H 0 z
                        m 0,2 H 10 v 1 H 0 z
                        m 0,2 H 10 v 1 H 0 z
                        m 0,2 H 10 v 1 H 0 z
                        m 0,2 H 10 v 1 H 0 z
                        M 1,0 V 10 h 1 V 0 z
                        m 2,0 V 10 h 1 V 0 z
                        m 2,0 V 10 h 1 V 0 z
                        m 2,0 V 10 h 1 V 0 z
                        m 2,0 V 10 h 1 V 0 z"
                />
            </svg>

            <Canvas
                className="pointer-events-none! absolute! inset-0 z-40
                    touch-none select-none"
                data-testid="boardEffects"
            >
                <BoardEffects />
            </Canvas>

            <HighlightedLegalMovesRenderer />
            <EmphasizedSquaresRenderer />
            <IntermediateSquarePrompt />
            <LastMoveHighlight />
            <OverlayRenderer />
            <PromotionPrompt />
            <PieceRenderer />
            <ThrowPrompt />
            <Coords />
            {children}
        </div>
    );
};
export default ChessboardLayout;
