"use client";

import { StoreApi } from "zustand";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import ChessboardWithSidebar from "@/features/chessboard/components/ChessboardWithSidebar";
import ChessboardLayout from "@/features/chessboard/components/ChessboardLayout";
import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import processRootAnalysis from "../lib/rootAnalysisPositionProcessor";
import useAnalysisMoveResolver from "../hooks/useAnalysisMoveResolver";
import { RootAnalysisPosition } from "@/lib/apiClient";
import AnalysisSide from "./AnalysisSide";
import useConst from "@/hooks/useConst";

const AnalysisChessboard = ({
    rootPosition,
}: {
    rootPosition: RootAnalysisPosition;
}) => {
    const chessboardStore = useConst<StoreApi<ChessboardStore>>(() =>
        processRootAnalysis(rootPosition),
    );
    useAnalysisMoveResolver(chessboardStore);

    return (
        <ChessboardStoreContext.Provider value={chessboardStore}>
            <ChessboardWithSidebar
                chessboard={
                    <ChessboardLayout
                        breakpoints={[
                            {
                                maxScreenSize: 768,
                                paddingOffset: { width: 40, height: 110 },
                            },
                            {
                                maxScreenSize: 1024,
                                paddingOffset: { width: 200, height: 50 },
                            },
                        ]}
                        defaultOffset={{ width: 626, height: 40 }}
                    />
                }
                aside={
                    <aside
                        className="flex min-h-96 w-full overflow-auto md:h-full
                            lg:max-w-sm"
                    >
                        <AnalysisSide />
                    </aside>
                }
            />
        </ChessboardStoreContext.Provider>
    );
};
export default AnalysisChessboard;
