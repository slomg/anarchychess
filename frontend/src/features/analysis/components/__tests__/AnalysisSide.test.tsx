import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import { mockScrollTo } from "@/lib/testUtils/mocks/mockDom";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import AnalysisSide from "../AnalysisSide";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

describe("AnalysisSide", () => {
    let chessboardStore: StoreApi<ChessboardStore>;

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

        const toolbar = screen.getByTestId("moveHistoryToolbar");
        expect(toolbar).toBeInTheDocument();

        expect(toolbar).toHaveClass("order-1 lg:order-2");
        expect(within(toolbar).getByTitle("Go to Start")).toBeInTheDocument();
        expect(
            within(toolbar).getByTitle("Setup Position"),
        ).toBeInTheDocument();
        expect(within(toolbar).getByTitle("Flip Board")).toBeInTheDocument();

        const rows = screen.getByTestId("moveHistoryRows");
        expect(rows).toBeInTheDocument();
        expect(rows).toHaveClass("order-2 lg:order-1");

        expect(
            screen.queryByTestId("analysisPositionSetup"),
        ).not.toBeInTheDocument();
    });

    it("should navigate to setup page and render setup layout with updated toolbar", async () => {
        const user = userEvent.setup();

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <AnalysisSide />
            </ChessboardStoreContext.Provider>,
        );

        await user.click(screen.getByTitle("Setup Position"));

        const toolbar = screen.getByTestId("moveHistoryToolbar");
        expect(toolbar).toBeInTheDocument();
        expect(toolbar).toHaveClass("order-1 lg:order-2");
        expect(
            within(toolbar).queryByTitle("Go to Start"),
        ).not.toBeInTheDocument();
        expect(within(toolbar).getByTitle("Flip Board")).toBeInTheDocument();
        const backButton = within(toolbar).getByTitle("Go Back");
        expect(backButton).toBeInTheDocument();

        const positionSetup = screen.getByTestId("analysisPositionSetup");
        expect(positionSetup).toBeInTheDocument();
        expect(positionSetup).toHaveClass("order-2 lg:order-1");

        expect(screen.queryByTestId("moveHistoryRows")).not.toBeInTheDocument();

        await user.click(backButton);
        expect(screen.getByTestId("moveHistoryRows")).toBeInTheDocument();
        expect(positionSetup).not.toBeInTheDocument();
    });
});
