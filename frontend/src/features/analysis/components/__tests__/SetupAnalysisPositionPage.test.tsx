import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import { mockScrollTo } from "@/lib/testUtils/mocks/mockDom";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { render, screen, within } from "@testing-library/react";
import SetupAnalysisPositionPage from "../SetupAnalysisPositionPage";
import userEvent from "@testing-library/user-event";
import { AnalysisPageType } from "../AnalysisSide";

describe("SetupAnalysisPositionPage", () => {
    let chessboardStore: StoreApi<ChessboardStore>;
    const setSelectedPageMock = vi.fn();

    beforeEach(() => {
        mockScrollTo();
        chessboardStore = createChessboardStore();
    });

    it("should render move history toolbar with the correct buttons", () => {
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <SetupAnalysisPositionPage
                    setSelectedPage={setSelectedPageMock}
                />
            </ChessboardStoreContext.Provider>,
        );

        const toolbar = screen.getByTestId("moveHistoryToolbar");
        expect(toolbar).toBeInTheDocument();

        expect(toolbar).toHaveClass("order-1 lg:order-2");
        expect(within(toolbar).getByTitle("Go Back")).toBeInTheDocument();
        expect(within(toolbar).getByTitle("Flip Board")).toBeInTheDocument();

        expect(setSelectedPageMock).not.toHaveBeenCalled();
    });

    it("should change page when clicking go back", async () => {
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <SetupAnalysisPositionPage
                    setSelectedPage={setSelectedPageMock}
                />
            </ChessboardStoreContext.Provider>,
        );

        await user.click(screen.getByTitle("Go Back"));

        expect(setSelectedPageMock).toHaveBeenCalledExactlyOnceWith(
            AnalysisPageType.Main,
        );
    });
});
