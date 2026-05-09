import { ChevronDownIcon, ChevronUpIcon } from "@heroicons/react/24/outline";
import Image from "next/image";
import { useId } from "react";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { pieceTypeToStr } from "@/features/chessboard/lib/pieceUtils";
import { getPieceImage } from "@/features/chessboard/lib/pieceUtils";
import Selector, { SelectorEvent } from "@/components/ui/Selector";
import { logicalToAlgebraic } from "@/features/point/pointUtils";
import InputField from "@/components/ui/InputField";

const SetupPositionPieceProperties = () => {
    const hasMovedId = useId();
    const stunnedForId = useId();

    const {
        selectedPieceId,
        setSetupModePieceHasMoved,
        setSetupModePieceStunned,
    } = useChessboardStore((x) => ({
        selectedPieceId: x.selectedPieceId,
        setSetupModePieceHasMoved: x.setSetupModePieceHasMoved,
        setSetupModePieceStunned: x.setSetupModePieceStunned,
    }));
    const pieces = useChessboardStore((x) => x.pieces);

    if (!selectedPieceId) {
        return null;
    }

    const piece = pieces.getById(selectedPieceId);
    if (!piece) {
        return null;
    }

    function handleHasMovedChange(event: SelectorEvent<boolean>) {
        if (selectedPieceId) {
            setSetupModePieceHasMoved(selectedPieceId, event.target.value);
        }
    }

    function handleStunnedChange(event: React.ChangeEvent<HTMLInputElement>) {
        if (!selectedPieceId) {
            return;
        }

        if (event.target.validity.badInput) {
            return;
        }

        if (event.target.value === "") {
            setSetupModePieceStunned(selectedPieceId, 0);
            return;
        }

        const stunnedForTurns = Math.max(0, parseInt(event.target.value));
        if (!isNaN(stunnedForTurns)) {
            setSetupModePieceStunned(selectedPieceId, stunnedForTurns);
            event.target.value = String(stunnedForTurns);
        }
    }

    function handleStunnedIncrement() {
        if (selectedPieceId && piece) {
            setSetupModePieceStunned(
                selectedPieceId,
                piece.stunnedForTurns + 1,
            );
        }
    }

    function handleStunnedDecrement() {
        if (selectedPieceId && piece) {
            setSetupModePieceStunned(
                selectedPieceId,
                Math.max(0, piece.stunnedForTurns - 1),
            );
        }
    }

    return (
        <div className="flex flex-col gap-3" data-testid="setupPieceProperties">
            <hr className="text-secondary/30" />

            <div className="flex w-full items-center">
                <div>
                    <h1 className="text-2xl">{pieceTypeToStr(piece.type)}</h1>
                    <p>
                        Piece properties on {logicalToAlgebraic(piece.position)}
                    </p>
                </div>

                <Image
                    src={getPieceImage(piece.type, piece.color)}
                    width={40}
                    height={40}
                    alt="Selected Piece"
                    unoptimized
                    className="ml-auto"
                />
            </div>

            <div>
                <label
                    className="text-text/90"
                    htmlFor={hasMovedId}
                    data-testid="formFieldLabel"
                >
                    Has moved
                </label>

                <Selector
                    className="h-min"
                    options={[
                        { label: "Yes", value: true },
                        { label: "No", value: false },
                    ]}
                    value={piece.hasMoved}
                    onChange={handleHasMovedChange}
                    id={hasMovedId}
                    data-testid="setupPiecePropertiesHasMoved"
                />
            </div>

            <div>
                <label
                    className="text-text/90"
                    htmlFor={stunnedForId}
                    data-testid="formFieldLabel"
                >
                    Stunned for (plies)
                </label>

                <InputField
                    as="input"
                    type="number"
                    pattern="[0-9]*"
                    value={piece.stunnedForTurns}
                    id={stunnedForId}
                    onChange={handleStunnedChange}
                    icon={
                        <div className="flex flex-col">
                            <button
                                onClick={handleStunnedIncrement}
                                className="text-text/50 hover:text-text w-min
                                    cursor-pointer"
                                data-testid="setupPiecePropertiesIncrementStunned"
                            >
                                <ChevronUpIcon className="h-4 w-4" />
                            </button>

                            <button
                                onClick={handleStunnedDecrement}
                                className="text-text/50 hover:text-text w-min
                                    cursor-pointer"
                                data-testid="setupPiecePropertiesDecrementStunned"
                            >
                                <ChevronDownIcon className="h-4 w-4" />
                            </button>
                        </div>
                    }
                />
            </div>
        </div>
    );
};
export default SetupPositionPieceProperties;
