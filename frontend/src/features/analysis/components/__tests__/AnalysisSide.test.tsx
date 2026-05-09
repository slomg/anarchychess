import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import * as SetupAnalysisPositionPage from "../SetupAnalysisPositionPage";
import * as MainAnalysisSidePage from "../MainAnalysisSidePage";
import { mockScrollTo } from "@/lib/testUtils/mocks/mockDom";
import AnalysisSide from "../AnalysisSide";

describe("AnalysisSide", () => {
    let chessboardStore: StoreApi<ChessboardStore>;

    const mainAnalysisSidePageMock = vi.spyOn(MainAnalysisSidePage, "default");
    const setupAnalysisPositionPageMock = vi.spyOn(
        SetupAnalysisPositionPage,
        "default",
    );

    beforeEach(() => {
        mockScrollTo();
        chessboardStore = createChessboardStore();
    });

    it("should render the correct main view and toolbar for the main page", () => {
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <AnalysisSide />
            </ChessboardStoreContext.Provider>,
        );

        expect(mainAnalysisSidePageMock).toHaveBeenCalled();
        expect(setupAnalysisPositionPageMock).not.toHaveBeenCalled();
    });

    it("should navigate to setup page and render setup layout with updated toolbar", async () => {
        const user = userEvent.setup();

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <AnalysisSide />
            </ChessboardStoreContext.Provider>,
        );

        mainAnalysisSidePageMock.mockClear();
        setupAnalysisPositionPageMock.mockClear();
        await user.click(screen.getByTitle("Setup Position"));

        expect(setupAnalysisPositionPageMock).toHaveBeenCalled();
        expect(mainAnalysisSidePageMock).not.toHaveBeenCalled();

        mainAnalysisSidePageMock.mockClear();
        setupAnalysisPositionPageMock.mockClear();
        await user.click(screen.getByTitle("Go Back"));

        expect(setupAnalysisPositionPageMock).not.toHaveBeenCalled();
        expect(mainAnalysisSidePageMock).toHaveBeenCalled();
    });
});
