import { WrenchScrewdriverIcon } from "@heroicons/react/24/outline";

import MoveHistoryToolbar from "@/features/chessboard/components/moveHistory/MoveHistoryToolbar";
import NavigationButtons from "@/features/chessboard/components/moveHistory/NavigationButtons";
import MoveHistoryRows from "@/features/chessboard/components/moveHistory/MoveHistoryRows";
import FlipButton from "@/features/chessboard/components/moveHistory/FlipButton";
import { AnalysisPageType } from "./AnalysisSide";
import Button from "@/components/ui/Button";

const MainAnalysisSidePage = ({
    setSelectedPage,
}: {
    setSelectedPage: (page: AnalysisPageType) => void;
}) => {
    return (
        <>
            <MoveHistoryToolbar
                className="order-1 lg:order-2"
                leftActions={<NavigationButtons />}
                rightActions={
                    <>
                        <Button
                            title="Setup Position"
                            onClick={() =>
                                setSelectedPage(AnalysisPageType.PositionSetup)
                            }
                        >
                            <WrenchScrewdriverIcon className="h-8 w-8" />
                        </Button>
                        <FlipButton />
                    </>
                }
            />

            <MoveHistoryRows className="order-2 lg:order-1" />
        </>
    );
};
export default MainAnalysisSidePage;
