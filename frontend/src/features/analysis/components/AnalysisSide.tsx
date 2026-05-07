import { MagnifyingGlassPlusIcon } from "@heroicons/react/24/outline";
import { useState } from "react";

import SetupAnalysisPositionPage from "./SetupAnalysisPositionPage";
import MainAnalysisSidePage from "./MainAnalysisSidePage";
import Card from "@/components/ui/Card";

export enum AnalysisPageType {
    Main,
    PositionSetup,
}

const AnalysisSide = () => {
    const [selectedPage, setSelectedPage] = useState<AnalysisPageType>(
        AnalysisPageType.Main,
    );

    let page: React.ReactNode;
    switch (selectedPage) {
        case AnalysisPageType.PositionSetup:
            page = (
                <SetupAnalysisPositionPage setSelectedPage={setSelectedPage} />
            );
            break;
        default:
            page = <MainAnalysisSidePage setSelectedPage={setSelectedPage} />;
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

            {page}
        </Card>
    );
};
export default AnalysisSide;
