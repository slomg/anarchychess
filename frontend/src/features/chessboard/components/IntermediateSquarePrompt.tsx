import { useRef } from "react";

import { useChessboardStore } from "../hooks/useChessboard";
import { LogicalPoint } from "@/features/point/types";
import ChessSquare from "./ChessSquare";

const IntermediateSquarePrompt = () => {
    const { nextIntermediates, resolveNextIntermediate } = useChessboardStore(
        (x) => ({
            nextIntermediates: x.nextIntermediates,
            resolveNextIntermediate: x.resolveNextIntermediate,
        }),
    );
    if (nextIntermediates.length === 0) return null;

    return (
        <div
            data-testid="intermediateSquarePromptOverlay"
            className="absolute inset-0 z-50 flex cursor-auto bg-black/50"
            onPointerDown={(e) => {
                resolveNextIntermediate?.(null);
                e.stopPropagation();
            }}
        >
            {nextIntermediates.map((point, i) => (
                <IntermediateSquare point={point} key={i} />
            ))}
        </div>
    );
};
export default IntermediateSquarePrompt;

const IntermediateSquare = ({ point }: { point: LogicalPoint }) => {
    const resolveNextIntermediate = useChessboardStore(
        (x) => x.resolveNextIntermediate,
    );
    const hadPointerDown = useRef(false);

    function handlePointerUp(event: React.PointerEvent<HTMLDivElement>) {
        event.stopPropagation();
        if (hadPointerDown.current) {
            resolveNextIntermediate?.(point);
        }
    }

    return (
        <ChessSquare
            data-testid="intermediateSquare"
            position={point}
            onPointerDown={(e) => {
                hadPointerDown.current = true;
                e.stopPropagation();
            }}
            onPointerUp={handlePointerUp}
            className="border-accent z-30 cursor-pointer rounded-sm border-4
                bg-[length:100%_100%] bg-center bg-no-repeat transition-colors
                duration-100 ease-out hover:bg-[rgba(255,215,0,0.2)]"
        />
    );
};
