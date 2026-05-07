import { render, screen, within } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { mockScrollTo } from "@/lib/testUtils/mocks/mockDom";
import MoveHistoryTable from "../MoveHistoryTable";

describe("MoveHistoryTable", () => {
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        mockScrollTo();
        chessboardStore = createChessboardStore();
    });

    it("should render move history toolbar with the correct buttons", () => {
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryTable />
            </ChessboardStoreContext.Provider>,
        );

        const toolbar = screen.getByTestId("moveHistoryToolbar");
        expect(toolbar).toBeInTheDocument();

        expect(toolbar).toHaveClass("order-1 lg:order-2");
        expect(within(toolbar).getByTitle("Go to Start")).toBeInTheDocument();
        expect(within(toolbar).getByTitle("Flip Board")).toBeInTheDocument();
    });

    it("should render move history rows", () => {
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryTable />
            </ChessboardStoreContext.Provider>,
        );

        const rows = screen.getByTestId("moveHistoryRows");
        expect(rows).toBeInTheDocument();
        expect(rows).toHaveClass("order-2 lg:order-1");
    });
});
