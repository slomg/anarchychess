import { ArrowsUpDownIcon } from "@heroicons/react/24/outline";

import { useChessboardStore } from "../../hooks/useChessboard";
import Button from "@/components/ui/Button";

const FlipButton = () => {
    const { flipBoard } = useChessboardStore((x) => ({
        flipBoard: x.flipBoard,
    }));

    return (
        <Button onClick={flipBoard} title="Flip Board">
            <ArrowsUpDownIcon className="h-8 w-8" />
        </Button>
    );
};
export default FlipButton;
