import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import MoveHistoryTable from "../MoveHistoryTable";
import {
    createFakePosition,
    createFakeStartingPosition,
} from "@/lib/testUtils/fakers/positionFaker";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import userEvent from "@testing-library/user-event";
import { mockScrollTo } from "@/lib/testUtils/mocks/mockDom";

describe("MoveHistoryTable", () => {
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        mockScrollTo();
        chessboardStore = createChessboardStore();
    });

    function renderWithCtx() {
        return render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveHistoryTable />
            </ChessboardStoreContext.Provider>,
        );
    }

    it("should render an empty table when there are no moves", () => {
        renderWithCtx();
        const rows = screen.queryAllByRole("row");
        expect(rows.length).toBe(0);
    });

    it("should render a single row when there is one move", () => {
        chessboardStore.setState({
            positionHistory: [
                createFakeStartingPosition(),
                createFakePosition({ san: "e4" }),
            ],
        });

        renderWithCtx();

        expect(screen.getByText("1.")).toBeInTheDocument();
        expect(screen.getByText("e4")).toBeInTheDocument();
    });

    it("should render multiple rows for multiple moves", () => {
        chessboardStore.setState({
            positionHistory: [
                createFakeStartingPosition(),
                createFakePosition({ san: "e4" }),
                createFakePosition({ san: "e5" }),
                createFakePosition({ san: "Nf3" }),
                createFakePosition({ san: "Nc6" }),
            ],
        });

        renderWithCtx();

        expect(screen.getByText("1.")).toBeInTheDocument();
        expect(screen.getByText("2.")).toBeInTheDocument();
        expect(screen.getByText("e4")).toBeInTheDocument();
        expect(screen.getByText("e5")).toBeInTheDocument();
        expect(screen.getByText("Nf3")).toBeInTheDocument();
        expect(screen.getByText("Nc6")).toBeInTheDocument();
    });

    it("should apply alternating background color class for odd rows", () => {
        chessboardStore.setState({
            positionHistory: [
                createFakeStartingPosition(),
                createFakePosition({ san: "e4" }),
                createFakePosition({ san: "e5" }),
                createFakePosition({ san: "Nf3" }),
                createFakePosition({ san: "Nf6" }),
            ],
        });

        renderWithCtx();

        const rows = screen.getAllByRole("row");
        expect(rows.length).toBe(2);

        expect(rows[0].className).not.toContain("bg-white/10");
        expect(rows[1].className).toContain("bg-white/10");
    });

    it("should update position using arrow keys", async () => {
        const position1 = createFakeStartingPosition();
        const position2 = createFakePosition();
        const position3 = createFakePosition();

        chessboardStore.setState({
            viewingPlyIdx: 0,
            positionHistory: [position1, position2, position3],
        });

        const user = userEvent.setup();
        renderWithCtx();

        // go to move 2
        await user.keyboard("{ArrowRight}");
        expect(chessboardStore.getState().pieces).toEqual(position2.pieces);

        // go to move 3
        await user.keyboard("{ArrowRight}");
        expect(chessboardStore.getState().pieces).toEqual(position3.pieces);

        // go back to move 2
        await user.keyboard("{ArrowLeft}");
        expect(chessboardStore.getState().pieces).toEqual(position2.pieces);

        // jump to end
        await user.keyboard("{ArrowDown}");
        expect(chessboardStore.getState().pieces).toEqual(position3.pieces);

        // jump to start
        await user.keyboard("{ArrowUp}");
        expect(chessboardStore.getState().pieces).toEqual(position1.pieces);
    });

    it("should update position when clicking on a move", async () => {
        const position1 = createFakeStartingPosition();
        const position2 = createFakePosition({ san: "e4" });
        const position3 = createFakePosition({ san: "e5" });
        const position4 = createFakePosition({ san: "e6" });

        chessboardStore.setState({
            positionHistory: [position1, position2, position3, position4],
        });

        const user = userEvent.setup();
        renderWithCtx();

        await user.click(screen.getByText("e4"));
        expect(chessboardStore.getState().pieces).toEqual(position2.pieces);

        await user.click(screen.getByText("e5"));
        expect(chessboardStore.getState().pieces).toEqual(position3.pieces);

        await user.click(screen.getByText("e6"));
        expect(chessboardStore.getState().pieces).toEqual(position4.pieces);
    });

    it("should render game actions", () => {
        renderWithCtx();
        expect(screen.getByTestId("gameActions")).toBeInTheDocument();
    });
});
