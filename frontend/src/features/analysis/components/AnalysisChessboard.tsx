"use client";

import ChessboardLayout from "@/features/chessboard/components/ChessboardLayout";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import useConst from "@/hooks/useConst";
import { RootAnalysisPosition } from "@/lib/apiClient";
import { StoreApi } from "zustand";
import processRootAnalysis from "../lib/rootAnalysisPositionProcessor";
import useAnalysisMoveResolver from "../hooks/useAnalysisMoveResolver";
import ChessboardWithSidebar from "@/features/chessboard/components/ChessboardWithSidebar";
import MoveHistoryTable from "@/features/chessboard/components/moveHistory/MoveHistoryTable";
import { MagnifyingGlassPlusIcon } from "@heroicons/react/24/solid";

const AnalysisChessboard = ({
    rootPosition,
}: {
    rootPosition: RootAnalysisPosition;
}) => {
    const chessboardStore = useConst<StoreApi<ChessboardStore>>(() =>
        processRootAnalysis(rootPosition),
    );
    useAnalysisMoveResolver(rootPosition, chessboardStore);

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
                        className="flex h-96 min-h-25 w-full overflow-auto
                            md:h-full lg:max-w-sm"
                    >
                        <MoveHistoryTable
                            title={
                                <>
                                    <MagnifyingGlassPlusIcon className="h-7 w-7" />
                                    <h1>Analysis</h1>
                                </>
                            }
                        />
                    </aside>
                }
            />
        </ChessboardStoreContext.Provider>
    );
};
export default AnalysisChessboard;
