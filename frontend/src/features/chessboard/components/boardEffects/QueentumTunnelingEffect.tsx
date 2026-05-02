import { LogicalPoint } from "@/features/point/types";
import { TransientBoardEffectType } from "../../stores/boardEffectsSlice";

export interface QueentumTunnelingEffect {
    type: TransientBoardEffectType.QUEENTUM_TUNNELLING;
    at1: LogicalPoint;
    at2: LogicalPoint;
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
    return null;
};
export default QueentumTunnelingEffect;
