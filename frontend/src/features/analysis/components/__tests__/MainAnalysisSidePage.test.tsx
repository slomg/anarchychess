import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { mockScrollTo } from "@/lib/testUtils/mocks/mockDom";
import MainAnalysisSidePage from "../MainAnalysisSidePage";
import { AnalysisPageType } from "../AnalysisSide";

describe("MainAnalysisSidePage", () => {
    let chessboardStore: StoreApi<ChessboardStore>;
    const setSelectedPageMock = vi.fn();

    beforeEach(() => {
        mockScrollTo();
        chessboardStore = createChessboardStore();
    });

    it("should render move history toolbar with the correct buttons", () => {
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MainAnalysisSidePage setSelectedPage={setSelectedPageMock} />
            </ChessboardStoreContext.Provider>,
        );

        const toolbar = screen.getByTestId("moveHistoryToolbar");
        expect(toolbar).toBeInTheDocument();

        expect(toolbar).toHaveClass("order-1 lg:order-2");
        expect(within(toolbar).getByTitle("Go to Start")).toBeInTheDocument();
        expect(within(toolbar).getByTitle("Flip Board")).toBeInTheDocument();
        expect(
            within(toolbar).getByTitle("Setup Position"),
        ).toBeInTheDocument();

        expect(setSelectedPageMock).not.toHaveBeenCalled();
    });

    it("should render move history rows", () => {
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MainAnalysisSidePage setSelectedPage={setSelectedPageMock} />
            </ChessboardStoreContext.Provider>,
        );

        const rows = screen.getByTestId("moveHistoryRows");
        expect(rows).toBeInTheDocument();
        expect(rows).toHaveClass("order-2 lg:order-1");
    });

    it("should change page when clicking on setup position button", async () => {
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MainAnalysisSidePage setSelectedPage={setSelectedPageMock} />
            </ChessboardStoreContext.Provider>,
        );

        await user.click(screen.getByTitle("Setup Position"));

        expect(setSelectedPageMock).toHaveBeenCalledExactlyOnceWith(
            AnalysisPageType.PositionSetup,
        );
    });
});
