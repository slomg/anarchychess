import {
    ChevronRightIcon,
    ChevronDoubleLeftIcon,
    ChevronDoubleRightIcon,
    ChevronLeftIcon,
} from "@heroicons/react/24/outline";

import { useChessboardStore } from "../../hooks/useChessboard";
import Button from "@/components/ui/Button";

const NavigationButtons = () => {
    const {
        stepPositionForward,
        stepPositionBackward,
        goToStartPosition,
        goToLatestPosition,
    } = useChessboardStore((x) => ({
        stepPositionForward: x.stepPositionForward,
        stepPositionBackward: x.stepPositionBackward,
        goToStartPosition: x.goToStartPosition,
        goToLatestPosition: x.goToLatestPosition,
        flipBoard: x.flipBoard,
    }));

    return (
        <>
            <Button onClick={goToStartPosition} title="Go to Start">
                <ChevronDoubleLeftIcon className="h-8 w-8" />
            </Button>
            <Button onClick={stepPositionBackward} title="Previous Move">
                <ChevronLeftIcon className="h-8 w-8" />
            </Button>
            <Button onClick={stepPositionForward} title="Next Move">
                <ChevronRightIcon className="h-8 w-8" />
            </Button>
            <Button onClick={goToLatestPosition} title="Go to End">
                <ChevronDoubleRightIcon className="h-8 w-8" />
            </Button>
        </>
    );
};
export default NavigationButtons;
