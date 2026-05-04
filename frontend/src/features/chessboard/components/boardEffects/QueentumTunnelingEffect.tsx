import { TransientBoardEffectType } from "../../stores/boardEffectsSlice";
import { LogicalPoint } from "@/features/point/types";
import { GameColor, PieceType } from "@/lib/apiClient";
import PieceWarpEffect from "./PieceWarpEffect";

export interface QueentumTunnelingEffect {
    type: TransientBoardEffectType.QUEENTUM_TUNNELLING;
    queenPosition: LogicalPoint;
    antiqueenPosition: LogicalPoint;
    color: GameColor | null;
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
    return (
        <>
            <PieceWarpEffect
                pieceType={PieceType.QUEEN}
                pieceColor={effect.color}
                position={effect.queenPosition}
                onSettle={onSettle}
                onComplete={onComplete}
            />

            <PieceWarpEffect
                pieceType={PieceType.ANTIQUEEN}
                pieceColor={effect.color}
                position={effect.antiqueenPosition}
                onSettle={onSettle}
                onComplete={onComplete}
            />
        </>
    );
};
export default QueentumTunnelingEffect;
