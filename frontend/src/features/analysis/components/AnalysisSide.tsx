import {
    ArrowUturnLeftIcon,
    MagnifyingGlassPlusIcon,
    WrenchScrewdriverIcon,
} from "@heroicons/react/24/outline";

import { useState } from "react";

import MoveHistoryToolbar from "@/features/chessboard/components/moveHistory/MoveHistoryToolbar";
import NavigationButtons from "@/features/chessboard/components/moveHistory/NavigationButtons";
import MoveHistoryRows from "@/features/chessboard/components/moveHistory/MoveHistoryRows";
import FlipButton from "@/features/chessboard/components/moveHistory/FlipButton";
import AnalysisPositionSetup from "./AnalysisPositionSetup";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";

enum AnalysisPage {
    Main,
    PositionSetup,
}

const AnalysisSide = () => {
    const [selectedPage, setSelectedPage] = useState<AnalysisPage>(
        AnalysisPage.Main,
    );

    let toolbar: React.ReactNode;
    let mainView: React.ReactNode;
    switch (selectedPage) {
        case AnalysisPage.PositionSetup:
            toolbar = (
                <MoveHistoryToolbar
                    className="order-1 lg:order-2"
                    leftActions={
                        <Button
                            title="Go Back"
                            onClick={() => setSelectedPage(AnalysisPage.Main)}
                        >
                            <ArrowUturnLeftIcon className="h-8 w-8" />
                        </Button>
                    }
                    rightActions={<FlipButton />}
                />
            );
            mainView = <AnalysisPositionSetup className="order-2 lg:order-1" />;
            break;
        default:
            toolbar = (
                <MoveHistoryToolbar
                    className="order-1 lg:order-2"
                    leftActions={<NavigationButtons />}
                    rightActions={
                        <>
                            <Button
                                title="Setup Position"
                                onClick={() =>
                                    setSelectedPage(AnalysisPage.PositionSetup)
                                }
                            >
                                <WrenchScrewdriverIcon className="h-8 w-8" />
                            </Button>
                            <FlipButton />
                        </>
                    }
                />
            );
            mainView = <MoveHistoryRows className="order-2 lg:order-1" />;
    }

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

            {toolbar}
            {mainView}
        </Card>
    );
};
export default AnalysisSide;
