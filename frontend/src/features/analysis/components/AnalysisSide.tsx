import {
    MagnifyingGlassPlusIcon,
    WrenchScrewdriverIcon,
} from "@heroicons/react/24/outline";

import MoveHistoryToolbar from "@/features/chessboard/components/moveHistory/MoveHistoryToolbar";
import MoveHistoryRows from "@/features/chessboard/components/moveHistory/MoveHistoryRows";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import FlipButton from "@/features/chessboard/components/moveHistory/FlipButton";
import NavigationButtons from "@/features/chessboard/components/moveHistory/NavigationButtons";

const AnalysisSide = () => {
    return (
        <Card className="flex-1 gap-0 overflow-hidden p-0">
            <div
                className="bg-primary flex items-center justify-center gap-1
                    rounded-t-md p-1 text-2xl"
                data-testid="moveHistoryTitle"
            >
                <MagnifyingGlassPlusIcon className="h-7 w-7" />
                <h1>Analysis</h1>
            </div>

            <MoveHistoryToolbar
                className="order-1 lg:order-2"
                leftActions={<NavigationButtons />}
                rightActions={
                    <>
                        <Button>
                            <WrenchScrewdriverIcon className="h-8 w-8" />
                        </Button>
                        <FlipButton />
                    </>
                }
            />
            <MoveHistoryRows className="order-2 lg:order-1" />
        </Card>
    );
};
export default AnalysisSide;
