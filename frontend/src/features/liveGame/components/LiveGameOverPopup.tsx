import useMatchmaking from "@/features/lobby/hooks/useMatchmaking";
import useLiveChessStore from "../hooks/useLiveChessStore";
import useRematch from "../hooks/useRematch";
import GameOverPopup from "./GameOverPopup";
import Button from "@/components/ui/Button";
import clsx from "clsx";

const LiveGameOverPopup = () => {
    const viewer = useLiveChessStore((x) => x.viewer);
    const pool = useLiveChessStore((x) => x.pool);

    const { toggleSeek, isSeeking } = useMatchmaking(pool);

    return (
        <GameOverPopup
            controls={
                <>
                    <Button
                        className={clsx(
                            "flex-1",
                            isSeeking && "animate-breathe",
                        )}
                        onClick={toggleSeek}
                    >
                        {isSeeking ? "SEARCHING..." : "NEW GAME"}
                    </Button>

                    {viewer.playerColor !== null && <RematchButton />}
                </>
            }
        />
    );
};
export default LiveGameOverPopup;

const RematchButton = () => {
    const {
        toggleRematch,
        requestRematch,
        isRequestingRematch,
        isRematchRequested,
    } = useRematch();

    if (isRematchRequested) {
        return (
            <Button
                onClick={requestRematch}
                className="bg-secondary flex-1 text-black"
            >
                REMATCH?
            </Button>
        );
    } else {
        return (
            <Button
                onClick={toggleRematch}
                className={clsx(
                    "flex-1",
                    isRequestingRematch && "animate-breathe",
                )}
            >
                REMATCH
            </Button>
        );
    }
};
